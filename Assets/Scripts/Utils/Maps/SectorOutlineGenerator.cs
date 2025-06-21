using DIY_DOOM.Maps;
using DIY_DOOM.WADs.Data.Maps;

using System.Collections.Generic;

using UnityEngine;

using DIY_DOOM.MeshGeneration.Triangulation;
using static UnityEditor.Searcher.SearcherWindow.Alignment;
using DIY_DOOM.MeshGeneration.Triangulation.Base;
using static DIY_DOOM.Utils.Maps.SectorOutlineGenerator;


namespace DIY_DOOM.Utils.Maps
{
    /// <summary>
    /// This class uses the information from the map to determine the outline for the sector.
    /// It also handles holes in the sector.
    /// </summary>
    /// <remarks>
    /// The code in this class is based on the this algorithm.
    /// https://alienryderflex.com/triangulation/    
    /// </remarks>
    public static class SectorOutlineGenerator
    {
        private const int MAX_OUTLINE_ATTEMPTS = 10;


        private static Map _Map;
        private static int _SectorIndex;
        private static SectorDef _SectorDef;

        /// <summary>
        /// This dictionary holds the outline data for each hole that exists within this sector (aka another sector)
        /// </summary>
        private static Dictionary<int, HoleData> Holes = new Dictionary<int, HoleData>();

        /// <summary>
        /// This list contains the holes sorted based on the length of their shortest connector line segment (shortest to longest).
        /// </summary>
        private static List<HoleData> _SortedHolesList = new List<HoleData>();

        /// <summary>
        /// This list holds the outline of the sector.
        /// </summary>
        private static List<Vector2> SectorOutline = new List<Vector2>();



        private static List<LineDef> _AllSectorInFacingLineDefs = new List<LineDef>();
        private static List<LineDef> _AllSectorOutFacingLineDefs = new List<LineDef>();
        private static List<LineDef> _AllSectorLineDefs = new List<LineDef>();



        /// <summary>
        /// Clears all previous generated data from the lists.
        /// </summary>
        private static void ClearAllLists()
        {
            Holes.Clear();
            SectorOutline.Clear();
        }

        /// <summary>
        /// Determines the outline of this sector.
        /// </summary>
        public static void DetermineOutline(Map map, int sectorIndex)
        {
            ClearAllLists();

            _Map = map;
            _SectorIndex = sectorIndex;
            _SectorDef = _Map.GetSectorDef((uint) _SectorIndex);

            if (_SectorDef == null)
            {
                Debug.LogError($"Invalid sector index ({_SectorIndex})!: Skipping outline determination for this sector.");
                return;
            }
            
            FindAllOutlines();

            Debug.Log($"Outlines: {Holes.Count}");
            // NOTE: This is supposed to be <= Holes.Count, since Holes is a dictionary and the key
            //       is a 1-based index.
            for (int i = 1; i <= Holes.Count; i++)
            {
                Debug.Log("Hole ID: " + Holes[i].ID);
            }

            if (Holes.Count < 1)
            {
                Debug.LogWarning("No complete outlines were found for this sector!");
                return;
            }


            // Find the largest outline. This one is the exterior walls of the sector.
            int idOfLargest = FindLargestOutline();
            if (idOfLargest <= 0)
            {
                Debug.LogError("Failed to find the largest outline for this sector!");
                return;
            }

            Debug.Log($"Largest: {idOfLargest}    {Holes[idOfLargest].Vertices.Count}");

            // Copy the largest outline into SectorOutline, and remove it from the holes list.
            SectorOutline.Clear();
            SectorOutline.AddRange(Holes[idOfLargest].Vertices);
            Holes.Remove(idOfLargest);

            // Incorporate holes into the outline so they will be included in the triangulation of the sector.
            int originalVerts = SectorOutline.Count;
            bool result;
            if (Holes.Count > 0)
            {
                result = IncorporateHoles();
            }

            // Check if the outline is clockwise.
            bool result2 = Triangulator_Polygon.GatherPolygonData(SectorOutline, out PolygonDetails polygonDetails);
            if (!result2)
            {
                Debug.LogError($"Failed to gather polygon data for Sector outline of sector with ID {_SectorDef.ID}!");
                return;
            }

            // If the newly generated sector outline is not in clockwise order, then reverse it.
            // This is necessary, as the floor/ceiling generation code will try to triangulate the outside of the outline
            // rather than the inside if the outline is counter-clockwise.
            if (!polygonDetails.IsClockwise)
            {
                Debug.Log("Sector outline is not clockwise! Reversing it.");
                SectorOutline.Reverse();
            }

            // Fill in the SectorOutline field in the SectorDef object.
            _SectorDef.SectorOutline.Clear();
            _SectorDef.SectorOutline.AddRange(SectorOutline);

            Debug.Log($"OrigVerts: {originalVerts}    NewVerts: {_SectorDef.SectorOutline.Count}");
        }

        private static int AddRangeWithoutDuplicates(List<LineDef> list, List<LineDef> rangeToAdd)
        {
            int c = 0;

            foreach (LineDef lineDef in rangeToAdd)
            {
                bool isDuplicate = false;

                
                for (int i = 0; i < list.Count; i++)
                {
                    if ((list[i].StartVertexID == lineDef.StartVertexID && list[i].EndVertexID == lineDef.EndVertexID) ||
                        (list[i].StartVertexID == lineDef.EndVertexID && list[i].EndVertexID == lineDef.StartVertexID))
                    {
                        isDuplicate = true;
                        c++;
                        break;
                    }
                } // end for i
                

                if (!isDuplicate)
                    list.Add(lineDef);

            } // end foreach lineDef


            Debug.Log($"Discarded {c} duplicate lineDefs.");

            return c;
        }

        /// <summary>
        /// Finds all outlines for this sector. Every sector will have at least one (its exterior walls).
        /// Some will have smaller sectors inside them, which will be completely seperate outlines.
        /// We need to sort through the segs data to construct these outlines.
        /// </summary>
        private static void FindAllOutlines()
        {
            float discarded = 0;


            ClearAllLineDefs();
            discarded += AddRangeWithoutDuplicates(_AllSectorInFacingLineDefs, _SectorDef.FrontLineDefs);
            discarded += AddRangeWithoutDuplicates(_AllSectorOutFacingLineDefs, _SectorDef.BackLineDefs);
            discarded += AddRangeWithoutDuplicates(_AllSectorLineDefs, _SectorDef.FrontLineDefs);
            discarded += AddRangeWithoutDuplicates(_AllSectorLineDefs, _SectorDef.BackLineDefs);

            Debug.Log($"<color=yellow>SECTOR: {_SectorIndex}</color>");
            Debug.Log($"Sector LineDefs: {_AllSectorLineDefs.Count} - {discarded} = {_AllSectorLineDefs.Count - discarded}");
            
            for (int i = 0; i < _AllSectorLineDefs.Count; i++)
            {
                LineDef lineDef = _AllSectorLineDefs[i];
                Debug.Log($"    Line[{i}] [{lineDef.StartVertexID}]{_Map.GetVertex(lineDef.StartVertexID), 30} {new string(' ', 20)} -> [{lineDef.EndVertexID}]{_Map.GetVertex(lineDef.EndVertexID), 30}");
                Debug.Log($"               {lineDef.FrontSideDefIndex}    {lineDef.BackSideDefIndex}");
            }
            

            if (_AllSectorLineDefs.Count < 1)
            {
                Debug.LogWarning("This sector has no line defs! Skipping it...");
                return;
            }

            int holeIndex = -1;
            int invalidOutlines = 0;
            Vector3 firstLineDefStartPoint = Vector3.zero;
            Vector3 lastLineDefEndPoint = Vector3.zero;


            while (_AllSectorLineDefs.Count > 0)
            {
                holeIndex++;

                firstLineDefStartPoint = _Map.GetVertex(_AllSectorLineDefs[0].StartVertexID);
                lastLineDefEndPoint = _Map.GetVertex(_AllSectorLineDefs[0].EndVertexID);
                RemoveLineDef(_AllSectorLineDefs[0]);

                HoleData newHoleData = new HoleData();
                newHoleData.Vertices = new List<Vector2>();
                newHoleData.ID = holeIndex + 1;
                newHoleData.Vertices.Add(MapUtils.Point3dTo2d(firstLineDefStartPoint));
                newHoleData.Vertices.Add(MapUtils.Point3dTo2d(lastLineDefEndPoint));


                bool curOutlineIsDone = false;
                while (!curOutlineIsDone)
                {
                    curOutlineIsDone = false;
                    
                    int nextIndex = FindNextLineDef(_AllSectorInFacingLineDefs, newHoleData, firstLineDefStartPoint, ref lastLineDefEndPoint);
                    if (nextIndex < 0)
                        nextIndex = FindNextLineDef(_AllSectorOutFacingLineDefs, newHoleData, firstLineDefStartPoint, ref lastLineDefEndPoint);


                    // Did we find the next segment in this outline?
                    if (nextIndex >= 0)
                    {
                        int vertCount = newHoleData.Vertices.Count;

                        // Is this the last seg in this outline?
                        if (lastLineDefEndPoint == firstLineDefStartPoint)
                        {
                            if (!IsValidNextLineDef(newHoleData.Vertices[vertCount - 1] - newHoleData.Vertices[vertCount - 2],
                                firstLineDefStartPoint - lastLineDefEndPoint))
                            {
                                Debug.Log("BREAK (INNER-INVALID END LINEDEF)");
                                break;
                            }

                            Debug.Log($"DONE (INNER)    {lastLineDefEndPoint == firstLineDefStartPoint}    {IsValidNextLineDef(newHoleData.Vertices[vertCount - 1] - newHoleData.Vertices[vertCount - 2], firstLineDefStartPoint - lastLineDefEndPoint)}");

                            // It is the last seg in this outline, so just break out of the inner while loop.
                            curOutlineIsDone = true;
                            break;
                        }

                    }
                    else
                    {
                        Debug.Log("BREAK (INNER)");
                        break;
                    }
                                        
                } // end while (!curOutlineIsDone)


                Triangulator_Polygon.GatherPolygonData(newHoleData.Vertices, out newHoleData.PolygonDetails);

                // If the current loop is less than three segments long, then it is invalid so discard it.
                if (IsValidHole(newHoleData, lastLineDefEndPoint, firstLineDefStartPoint))
                {
                    // This outline is finished, so add it into the Holes list, and then gather some basic information about it.
                    Holes.Add(newHoleData.ID, newHoleData);
                    
                    
                    // If this outline is not clockwise, then reverse it so it is.
                    if (!newHoleData.PolygonDetails.IsClockwise)
                    {
                        newHoleData.Vertices.Reverse();
                        newHoleData.PolygonDetails = new PolygonDetails(newHoleData.PolygonDetails.IsConvex,
                                                                        false,
                                                                        newHoleData.PolygonDetails.LeftTurns,
                                                                        newHoleData.PolygonDetails.RightTurns,
                                                                        newHoleData.PolygonDetails.ColinearSections);
                    }
                }
                else
                {
                    holeIndex--;
                    invalidOutlines++;
                }

                if (_AllSectorLineDefs.Count < 1)
                {
                    Debug.Log("BREAK (OUTER)");
                    break;
                }
            } // end while


            Debug.Log($"Holes Found: {Holes.Count}    Invalid Outlines Removed: {invalidOutlines}");
        }

        private static bool RemoveLineDef(LineDef lineDef)
        {
            bool removed = false;


            if (_AllSectorLineDefs.Contains(lineDef))
            {
                _AllSectorLineDefs.Remove(lineDef);
                removed = true;
            }

            if (_AllSectorInFacingLineDefs.Contains(lineDef))
            {
                _AllSectorInFacingLineDefs.Remove(lineDef);
                removed = true;
            }

            if (_AllSectorOutFacingLineDefs.Contains(lineDef))
            {
                _AllSectorOutFacingLineDefs.Remove(lineDef);
                removed = true;
            }


            return removed;
        }

        private static void ClearAllLineDefs()
        {
            _AllSectorInFacingLineDefs.Clear();
            _AllSectorOutFacingLineDefs.Clear();
            _AllSectorLineDefs.Clear();
        }

        /// <summary>
        /// This function goes through all of the outlines we found for this sector and its holes, and finds the biggest one (the exterior walls of the sector).
        /// </summary>
        private static int FindLargestOutline()
        {
            List<int> holeIDs = new List<int>();
            List<Vector2> minExtents = new List<Vector2>();
            List<Vector2> maxExtents = new List<Vector2>();


            // Iterate through all the outlines
            foreach (KeyValuePair<int, HoleData> pair in Holes)
            {
                Vector2 curMin = new Vector2(float.MaxValue, float.MaxValue);
                Vector2 curMax = new Vector2(float.MinValue, float.MinValue);

                HoleData curHoleData = pair.Value;

                Debug.Log($"HOLE[{curHoleData.ID}]");

                // Iterate through all of the vertices of this outline
                for (int j = 0; j < Holes[curHoleData.ID].Vertices.Count; j++)
                {
                    Vector2 vert = Holes[curHoleData.ID].Vertices[j];

                    Debug.Log($"    VERT[{j}]: {vert}");

                    // Compare to current min extents
                    if (vert.x < curMin.x)
                        curMin.x = vert.x;
                    if (vert.y < curMin.y)
                        curMin.y = vert.y;

                    // Compare to current max extents
                    if (vert.x > curMax.x)
                        curMax.x = vert.x;
                    if (vert.y > curMax.y)
                        curMax.y = vert.y;

                } // end for j


                holeIDs.Add(curHoleData.ID);
                minExtents.Add(curMin);
                maxExtents.Add(curMax);

            } // end for i


            List<float> areas = new List<float>();
            float largestArea = 0f;
            int indexOfLargest = -1;

            // Iterate through the lists of extents to see which outline is the largest.
            for (int k = 0; k < minExtents.Count; k++)
            {
                areas.Add(0);

                float width = maxExtents[k].x - minExtents[k].x;
                float depth = maxExtents[k].y - minExtents[k].y;

                // We're ignoring height here on purpose, since including it could cause a sector to be considered larger only because it is taller,
                // but we only care about the size horizontally.
                areas[k] = width * depth;

                // Is this the largest outline so far?
                if (areas[k] > largestArea)
                {
                    largestArea = areas[k];
                    indexOfLargest = k;
                }


                Debug.Log($"Extents:    {width}x{depth}={areas[k]}");

            } // end for k


            int largestHoleID = -1;
            if (indexOfLargest >= 0)
            {
                largestHoleID = holeIDs[indexOfLargest];
                Debug.Log($"IndexOfLargestArea:    {indexOfLargest}    ID: {largestHoleID}");
            }

            return largestHoleID;
        }

        private static int FindNextLineDef(List<LineDef> lineDefs, HoleData holeData, Vector3 firstLineDefStartPoint, ref Vector3 lastLineDefEndPoint)
        {
            int vertCount = holeData.Vertices.Count;

            for (int i = 0; i < lineDefs.Count; i++)
            {
                LineDef curLineDef = lineDefs[i];
                
                Vector3 startVertex = _Map.GetVertex(curLineDef.StartVertexID);
                Vector3 endVertex = _Map.GetVertex(curLineDef.EndVertexID);

                bool matched = false;
                Vector3 vertToAdd = Vector3.zero;
                Vector3 otherVert = Vector3.zero;


                // Is this the next seg in this outline?
                if (startVertex == lastLineDefEndPoint)
                {
                    matched = true;
                    vertToAdd = endVertex;
                    otherVert = startVertex;

                    Debug.Log($"LINEDEF MATCH [{i}]: START");
                }
                else if (endVertex == lastLineDefEndPoint)
                {
                    matched = true;
                    vertToAdd = startVertex;
                    otherVert = endVertex;

                    Debug.Log($"LINEDEF MATCH [{i}]: END");

                }
                else // No match was found
                {
                    //Debug.Log($"LINEDEF MISMATCH [{i}]");
                    continue;
                }


                Debug.Log($"{otherVert} | {vertToAdd}    ||    {firstLineDefStartPoint} | {lastLineDefEndPoint}");

                
                if (matched &&
                    IsValidNextLineDef(holeData.Vertices[vertCount - 1] - holeData.Vertices[vertCount - 2],
                                   vertToAdd - otherVert))
                {
                    // We found the next segment in this outline, so add it to the outline
                    // if the outline is not yet complete. If the start and end points are equal, then it is finished.
                    // In that case we don't add this point, since it is the same as the starting point.
                    if (vertToAdd != firstLineDefStartPoint)
                    {
                        holeData.Vertices.Add(MapUtils.Point3dTo2d(vertToAdd));
                    }


                    lastLineDefEndPoint = vertToAdd;

                    RemoveLineDef(lineDefs[i]);

                    // We found the next seg in this outline, so return it's index;
                    return i;
                }


            } // end for i


            // We failed to find the next seg in this outline, so return -1 to indicate this.
            return -1;
        }

        /// <summary>
        /// This function checks if the possible next seg we found is going back directly on top of the previous seg.
        /// In otherwords the angle of this corner is 180 degrees.
        /// </summary>
        /// <param name="lastSeg">The last segment we found so far.</param>
        /// <param name="possibleNextSeg">The possible next segment that needs to be checked.</param>
        /// <returns>True if the possible next seg is valid.</returns>
        private static bool IsValidNextLineDef(Vector3 lastLineDef, Vector3 possibleNextLineDef)
        {
            float dot = Vector3.Dot(lastLineDef.normalized, possibleNextLineDef.normalized);
            if (dot == -1.0f)
                return false;
            else
                return true;
        }

        private static bool IsValidHole(HoleData holeData, Vector3 lastLineDefEndPoint, Vector3 firstLineDefStartPoint)
        {
            Debug.Log($"<color=red>{holeData.Vertices == null} || {holeData.Vertices.Count < 3} || {holeData.PolygonDetails == null} || {lastLineDefEndPoint != firstLineDefStartPoint}</color>");
            // Does the hole's outline have at least 3 vertices, and form a complete circuit?
            if (holeData.Vertices == null || holeData.Vertices.Count < 3 || holeData.PolygonDetails == null || lastLineDefEndPoint != firstLineDefStartPoint)
                return false;


            // Is this hole's outline identical to an already existing hole in this sector?
            foreach (KeyValuePair<int, HoleData> pair in Holes)
            {
                HoleData curHole = pair.Value;

                if (curHole.ID == holeData.ID)
                    continue;

                if (curHole.Vertices.Count == holeData.Vertices.Count)
                {
                    int matches = 0;
                    for (int i = 0; i < curHole.Vertices.Count; i++)
                    {
                        for (int j = 0; j < holeData.Vertices.Count; j++)
                        {
                            if (holeData.Vertices[j] == curHole.Vertices[i])
                            {
                                matches++;
                                break;
                            }

                        } // end for j
                    } // end for i

                    if (matches >= curHole.Vertices.Count)
                    {
                        Debug.LogError("<color=blue>HOLE IS DUPLICATE!</color>");
                        return false;
                    }
                }

            } // end foreach


            return true;
        }


        // --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        // Functions for Incorporating Holes Into the Sector Outline
        // --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        // NOTE: The functions in this section are based on this algorithm for dealing with a polygon that has holes in it:
        //       https://alienryderflex.com/triangulation/

        /// <summary>
        /// This function iterates through all of the holes in the sector and incorporates them into the sector outline.
        /// This way the holes will be included when the sector is triangulated.
        /// </summary>
        /// <returns>True if successful.</returns>
        private static bool IncorporateHoles()
        {
            List<int> finishedHolesIDs = new List<int>();

            // Clear the sorted holes list, and then add each hole into it.
            _SortedHolesList.Clear();
            foreach (KeyValuePair<int, HoleData> pair in Holes)
            {
                _SortedHolesList.Add(pair.Value);
            }
            
            
            // Incorporate the hole outlines into the sector's outline one by one.
            while(true)
            {
                // First, we need to find out the closest vertex in each remaining hole to one of the sector outline's vertices.
                // This data will determine the order we incorporate the holes into the sector outline.
                // We do this every time the loop iterates, since the sector outline changes each time a hole is incorporated into it.
                // This will allow us to incorporate the holes into the sector outline without having problems where a previous
                // hole's connector is cutting through another hole that hasn't been incorporated yet.
                SortHolesByShortestConnectors(finishedHolesIDs);
                Debug.Log($"<color=cyan>Hole With Closest Connector ID: {_SortedHolesList[0].ID}    Length: {_SortedHolesList[0].ClosestConnectorData.Length}    {_SortedHolesList[0].ClosestConnectorData.HolePointIndex}    {_SortedHolesList[0].ClosestConnectorData.SectorOutlinePointIndex}</color>");
                // Connect the remaining hole with the shortest connector into the sector's outline.
                if (AddHoleToSectorOutline(_SortedHolesList[0]))
                    Debug.Log($"<color=magenta>Added Hole: {_SortedHolesList[0].ID}</color>");
                else
                    Debug.Log($"<color=magenta>Failed to Add Hole: {_SortedHolesList[0].ID}</color>");


                // Add this hole's ID to the list of finished holes IDs.
                finishedHolesIDs.Add(_SortedHolesList[0].ID);

                _SortedHolesList.RemoveAt(0);

                // If all holes have been incorporated into the sector's outline, then break out of this loop.
                if (/*finishedHolesIDs.Count >= Holes.Count ||*/ _SortedHolesList.Count < 1)
                    break;

            } // end while


            return true;
        }

        /// <summary>
        /// Adds the first hole in the sorted list (the one with the shortest connector line segment) into the sector's outline.
        /// </summary>
        /// <returns>True if successful.</returns>
        private static bool AddHoleToSectorOutline(HoleData holeData)
        {
            // Get the closest connector data for this hole.
            ClosestConnectorData closestConnectorData = holeData.ClosestConnectorData;

            // Copy the hole's vertex list so we can invert it quick since we need it to be counterclockwise.
            List<Vector2> holeVerts = new List<Vector2>(holeData.Vertices);

            // Reverse the order of the hole vertices, so we go counterclockwise was we add its vertices into the sector's outline.
            holeVerts.Reverse();



            // Duplicate the vertex we are connecting to in the sector outline.
            int insertionIndex = closestConnectorData.SectorOutlinePointIndex;
            SectorOutline.Insert(insertionIndex, SectorOutline[insertionIndex]);


            insertionIndex++;


            // Get the starting vertex of the hole, and shift it be subtracting it from the highest index. This adjustment is
            // needed since we are working with an inverted version of the vertices list for this hole since we have to
            // add the vertices to the sector outline in counterclockwise order.
            int holeStartIndex = Triangulator_Polygon.WrapIndex(holeVerts.Count - 1 - closestConnectorData.HolePointIndex, holeVerts.Count);

            // Insert the reversed hole vertices between the duplicated vertices we just made.
            for (int i = 0; i < holeVerts.Count; i++)
            {
                // I originally had this using - i, but that is wrong because we don't need to iterate backwards since
                // the temporary copy of the vertex list we're using is already inverted, and we already adjusted
                // the holeStartIndex accordingly.
                SectorOutline.Insert(insertionIndex, holeVerts[Triangulator_Polygon.WrapIndex(holeStartIndex + i, holeVerts.Count)]);
                insertionIndex++;

            } // end for i


            // Duplicate the first vertex at the end so that we connect the last vert to the first vert, before
            // then connecting back out to the sector outline.
            SectorOutline.Insert(insertionIndex, holeVerts[Triangulator_Polygon.WrapIndex(holeStartIndex, holeVerts.Count)]);


            return true;
        }

        /// <summary>
        /// Finds the closest line segment we can use to connect each hole to the outline.         
        /// </summary>
        private static bool SortHolesByShortestConnectors(List<int> finishedHolesIDs)
        {
            ResetAllEntriesInSortedList();


            // Iterate through each hole in the sector, and find the vertex that is closest to a vertex in the sector's outline.
            foreach (KeyValuePair<int, HoleData> pair in Holes)
            {
                HoleData curHoleData = pair.Value;


                // Skip this hole if it has already been incorporated into the sector's outline.                
                if (finishedHolesIDs.Contains(curHoleData.ID)) 
                { 
                    continue;
                }
                

                // Find the shortest line segment we can use to connect this hole to the sector's outline.
                if (FindShortestConnector(curHoleData))
                {
                    // If the sorted list's count is 1, then we don't need to worry about inserting this hole data in the correct place.
                    // Being the only item in the list, it already is in the right place.
                    if (_SortedHolesList.Count > 1)
                    {
                        // Remove this hole's data from the sorted list so we can reinsert it in the correct place.
                        _SortedHolesList.Remove(curHoleData);

                        // Insert this hole into the proper spot in the list based on the length of its shortest connector line segment.
                        bool inserted = false;
                        for (int i = 0; i < _SortedHolesList.Count; i++)
                        {
                            if (curHoleData.ClosestConnectorData.Length <= _SortedHolesList[i].ClosestConnectorData.Length)
                            {
                                _SortedHolesList.Insert(i, curHoleData);
                                inserted = true;
                                break;
                            }

                        } // end for i

                        // If the hole data wasn't reinserted in the list in the loop above, then we need to just add it to the end of the list, as that is its correct position.
                        if (!inserted)
                            _SortedHolesList.Add(curHoleData);
                    }
                }
                else
                {
                    Debug.Log($"<color=red>Failed to find shortest connector for hole {curHoleData.ID} in sector {_SectorDef.ID}!</color>");
                }


            } // end foreach


            return true;
        }

        /// <summary>
        /// Finds the shortest line segment we can create to connect the hole outline to the sector's outline.
        /// </summary>
        /// <returns>True if successful.</returns>
        private static bool FindShortestConnector(HoleData holeData)
        {
            List<Vector2> holeVerts = holeData.Vertices;


            // Iterate through all points in the hole.
            for (int i = 0; i < holeVerts.Count; i++)
            {
                Vector2 curHoleOutlinePoint = holeVerts[i];

                // Iterate through all vertices in the sector outline.
                for (int j = 0; j < SectorOutline.Count; j++)
                {
                    Vector2 curSectorOutlinePoint = SectorOutline[j];

                    // Check if this line segment would be the shortest possible connector so far.
                    float distance = Vector2.Distance(curHoleOutlinePoint, curSectorOutlinePoint);
                    if (distance < holeData.ClosestConnectorData.Length)
                    {
                        Debug.Log($"<color=green>[{holeData.ID}]    NEW SHORTEST: {distance}    OLD: {holeData.ClosestConnectorData.Length}    I: {i}    J: {j}</color>");

                        // Check if this line segment is a valid connector.
                        if (IsValidConnector(holeData, curHoleOutlinePoint, curSectorOutlinePoint))
                        {
                            holeData.ClosestConnectorData.Length = distance;
                            holeData.ClosestConnectorData.HolePointIndex = i;
                            holeData.ClosestConnectorData.SectorOutlinePointIndex = j;
                        }
                        else
                        {
                            Debug.Log("<color=magenta>INVALID CONNECTOR!</color>");
                            continue;
                        }
                    }

                } // end for j


            } // end for i


            return true;
        }

        /// <summary>
        /// Checks if this possible connector (line segment) intersects any line segments in the sector outline,
        /// or in any of the sector's hole outlines.
        /// </summary>
        /// <param name="holeData">The hole data of the hole we are working on.</param>
        /// <param name="holeOutlinePointIndex">The end of the connector that is attached to the hole's outline.</param>
        /// <param name="sectorOutlinePointIndex">The end of the connector that is attached to the sector's outline.</param>
        /// <returns>True if successful.</returns>
        private static bool IsValidConnector(HoleData holeData, Vector2 connectorStart, Vector2 connectorEnd)
        {
            Vector2 lineStart = Vector2.zero;
            Vector2 lineEnd = Vector2.zero;


            // Check if this line segment intersects with any line segments in any of the hole outlines,
            // including the hole it is connecting to.
            foreach (KeyValuePair<int, HoleData> pair in Holes)
            {
                HoleData curHole = pair.Value;
                

                //Debug.Log($"<color=yellow>HOLES COUNT: {Holes.Count}    {curHole.Vertices.Count}</color>");
                // Iterate through all of the line segments in this hole's outline to see if this potential connector intersects with any.
                for (int j = 0; j < curHole.Vertices.Count; j++)
                {
                    lineStart = curHole.Vertices[j];
                    lineEnd = curHole.Vertices[Triangulator_Polygon.WrapIndex(j + 1, curHole.Vertices.Count)];

                    bool result = Lines.GetLineSegmentIntersectionPoint(connectorStart, connectorEnd, lineStart, lineEnd, out Vector2 intersectionPoint, true);
                    //Debug.Log($"<color=orange>{connectorStart}-{connectorEnd}  |  {lineStart}-{lineEnd}  |  {result}    {intersectionPoint}</color>");
                    if (result)
                    {
                        if (curHole.ID == holeData.ID)
                            Debug.LogError("Connector intersects with the hole it is connecting!");
                        else
                            Debug.LogError("Connector intersects with another hole!");

                        return false;
                    }

                } // end for j

            } // end foreach


            // Iterate through all of the line segments in the sector's outline to see if this potential connector intersects with any.
            for (int k = 0; k < SectorOutline.Count; k++)
            {
                lineStart = SectorOutline[k];
                lineEnd = SectorOutline[Triangulator_Polygon.WrapIndex(k + 1, SectorOutline.Count)];

                if (Lines.GetLineSegmentIntersectionPoint(connectorStart, connectorEnd, lineStart, lineEnd, out _, true))
                {
                    Debug.LogError("Connector intersects with sector outline!");
                    return false;
                }

            } // end for k


            return true;
        }

        /// <summary>
        /// Resets every hole's ClosestConnectorData object.
        /// </summary>
        private static void ResetAllEntriesInSortedList()
        {
            for (int i = 0; i < _SortedHolesList.Count; i++)
            {
                _SortedHolesList[i].ClosestConnectorData.Reset();
            }
        }



        /// <summary>
        ///  Holds data about a hole in the sector.
        /// </summary>
        public class HoleData
        {
            public int ID;
            public List<Vector2> Vertices;

            /// <summary>
            /// Holds basic information about the hole's outline, such as whether or not its winding order is clockwise.
            /// </summary>
            public PolygonDetails PolygonDetails;

            /// <summary>
            /// Holds information on the shortest line segment we can use to connect the hole's outline to the sector's outline.
            /// </summary>
            public ClosestConnectorData ClosestConnectorData = new ClosestConnectorData();
        }


        /// <summary>
        /// Holds data about a possible connector (line segment) that could be used to connect a hole's outline to the sector's outline.
        /// </summary>
        public class ClosestConnectorData
        {
            public int SectorOutlinePointIndex = -1; // The index of the point that is on the sector's outline
            public int HolePointIndex = -1; // The index of the point that is on the hole's outline
            public float Length = float.MaxValue; // The length of this connector


            public void Reset()
            {
                SectorOutlinePointIndex = -1;
                HolePointIndex = -1;
                Length = float.MaxValue;
            }
        }
    }
}
