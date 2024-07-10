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


            FindAllOutlines();

            Debug.Log($"Outlines: {Holes.Count}");
            for (int i = 1; i <= Holes.Count; i++)
            {
                Debug.Log("ID: " + Holes[i].ID);
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
                Debug.LogError("INCORPORATING!");
                result = IncorporateHoles();
            }

            // Fill in the SectorOutline field in the SectorDef object.
            _SectorDef.SectorOutline.AddRange(SectorOutline);

            Debug.Log($"OrigVerts: {originalVerts}    NewVerts: {_SectorDef.SectorOutline.Count}");
        }

        private static int AddRangeWithoutDuplicates(List<SegDef> list, List<SegDef> rangeToAdd)
        {
            int c = 0;

            foreach (SegDef segDef in rangeToAdd)
            {
                bool isDuplicate = false;


                for (int i = 0; i < list.Count; i++)
                {
                    if ((list[i].StartVertexID == segDef.StartVertexID && list[i].EndVertexID == segDef.EndVertexID) ||
                        (list[i].StartVertexID == segDef.EndVertexID && list[i].EndVertexID == segDef.StartVertexID))
                    {
                        isDuplicate = true;
                        c++;
                        break;
                    }
                } // end for i


                if (!isDuplicate)
                    list.Add(segDef);

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

            List<SegDef> allSectorSegs = new List<SegDef>();
            // TODO: Remove the commented out lines below.
            discarded += AddRangeWithoutDuplicates(allSectorSegs, _SectorDef.FrontSegs);
            discarded += AddRangeWithoutDuplicates(allSectorSegs, _SectorDef.BackSegs);
            //allSectorSegs.AddRange(_SectorDef.FrontSegs);
            //allSectorSegs.AddRange(_SectorDef.BackSegs);

            Debug.Log("SECTOR: " + _SectorIndex);
            Debug.Log($"Sector Segs: {_SectorDef.FrontSegs.Count} + {_SectorDef.BackSegs.Count} = {_SectorDef.FrontSegs.Count + _SectorDef.BackSegs.Count} - {discarded} = {_SectorDef.FrontSegs.Count + _SectorDef.BackSegs.Count - discarded}    Actual: {allSectorSegs.Count}");
            for (int i = 0; i < allSectorSegs.Count; i++)
            {
                SegDef segDef = allSectorSegs[i];
                LineDef lineDef = _Map.GetLineDef(allSectorSegs[i].LineDefID);
                Debug.Log($"    Seg[{i}] ID={segDef.ID,5}    LineDefID: {segDef.LineDefID,6}    [{lineDef.StartVertexID}]{_Map.GetVertex(lineDef.StartVertexID), 30} {new string(' ', 20)} -> [{lineDef.EndVertexID}]{_Map.GetVertex(lineDef.EndVertexID), 30}");
                Debug.Log($"                      SegOffset: {segDef.Offset,6} {segDef.PercentStartShifted * 100}%    [{segDef.StartVertexID}]{_Map.GetVertex(segDef.StartVertexID), 30}{segDef.StartPoint} -> [{segDef.EndVertexID}]{_Map.GetVertex(segDef.EndVertexID), 30}    {lineDef.FrontSideDefIndex}    {lineDef.BackSideDefIndex}");
            }


            if (allSectorSegs.Count < 1)
            {
                Debug.LogWarning("This sector has no segs! Skipping it...");
                return;
            }

            int holeIndex = -1;
            int invalidOutlines = 0;
            Vector3 firstSegStartPoint = Vector3.zero;
            Vector3 lastSegEndPoint = Vector3.zero;
            int consecutiveFailedHoleOutlineAttempts = 0;

            while (allSectorSegs.Count > 0)
            {
                holeIndex++;

                firstSegStartPoint = _Map.GetVertex(allSectorSegs[0].StartVertexID);
                lastSegEndPoint = _Map.GetVertex(allSectorSegs[0].EndVertexID);
                allSectorSegs.RemoveAt(0);

                HoleData newHoleData = new HoleData();
                newHoleData.Vertices = new List<Vector2>();
                newHoleData.ID = holeIndex + 1;
                newHoleData.Vertices.Add(MapUtils.Point3dTo2d(firstSegStartPoint));
                newHoleData.Vertices.Add(MapUtils.Point3dTo2d(lastSegEndPoint));


                bool curOutlineIsDone = false;
                int failedOutlineAttempts = 0;
                while (!curOutlineIsDone)
                {
                    curOutlineIsDone = false;
                    int nextIndex = FindNextSeg(allSectorSegs, newHoleData, firstSegStartPoint, ref lastSegEndPoint);

                    // Did we find the next segment in this outline?
                    if (nextIndex >= 0)
                    {
                        // Is this the last seg in this outline?
                        if (lastSegEndPoint == firstSegStartPoint)
                        {
                            // It is the last seg in this outline, so just break out of the inner while loop.
                            curOutlineIsDone = true;
                            break;
                        }
                    }
                    else
                    {
                        failedOutlineAttempts++;
                    }

                    if (failedOutlineAttempts >= MAX_OUTLINE_ATTEMPTS)
                    {
                        Debug.Log("BREAK (INNER)");
                        break;
                    }
                } // end while (!curOutlineIsDone)


                // If the current loop is less than three segments long, then it is invalid so discard it.
                if (newHoleData.Vertices.Count >= 3)
                {
                    // This outline is finished, so add it into the Holes list, and then gather some basic information about it.
                    Holes.Add(newHoleData.ID, newHoleData);
                    Triangulator_Polygon.GatherPolygonData(newHoleData.Vertices, out newHoleData.PolygonDetails);

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

                    consecutiveFailedHoleOutlineAttempts++;
                    invalidOutlines++;
                }

                if (allSectorSegs.Count < 3)
                    break;


                if (consecutiveFailedHoleOutlineAttempts >= MAX_OUTLINE_ATTEMPTS / 2)
                {
                    Debug.Log("BREAK (OUTER)");
                    break;
                }
            } // end while


            Debug.Log($"Holes Found: {Holes.Count}    Invalid Outlines Removed: {invalidOutlines}");
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

        private static int FindNextSeg(List<SegDef> segs, HoleData holeData, Vector3 firstSegStartPoint, ref Vector3 lastSegEndPoint)
        {
            for (int i = 0; i < segs.Count; i++)
            {
                SegDef curSeg = segs[i];
                
                Vector3 startVertex = _Map.GetVertex(curSeg.StartVertexID);
                Vector3 endVertex = _Map.GetVertex(curSeg.EndVertexID);

                bool matched = false;
                Vector3 vertToAdd = Vector3.zero;
                Vector3 otherVert = Vector3.zero;


                // Is this the next seg in this outline?
                if (startVertex == lastSegEndPoint)
                {
                    matched = true;
                    vertToAdd = endVertex;
                    otherVert = startVertex;

                    Debug.Log($"SEG MATCH [{curSeg.ID}]: START");
                }
                else if (endVertex == lastSegEndPoint)
                {
                    matched = true;
                    vertToAdd = startVertex;
                    otherVert = endVertex;

                    Debug.Log($"SEG MATCH [{curSeg.ID}]: END");

                }
                // No match was found
                else
                {
                    Debug.Log($"SEG MISMATCH [{curSeg.ID}]");
                    continue;
                }


                Debug.Log($"{otherVert} | {vertToAdd}    ||    {firstSegStartPoint} | {lastSegEndPoint}");

                if (matched)
                {
                    // We found the next segment in this outline, so add it to the outline
                    // if the outline is not yet complete. If the start and end points are equal, then it is finished.
                    // In that case we don't add this point, since it is the same as the starting point.
                    if (vertToAdd != firstSegStartPoint)
                    {
                        holeData.Vertices.Add(MapUtils.Point3dTo2d(vertToAdd));
                    }


                    lastSegEndPoint = vertToAdd;

                    segs.RemoveAt(i);


                    // We found the next seg in this outline, so return it's index;
                    return i;
                }


            } // end for i


            // We failed to find the next seg in this outline, so return -1 to indicate this.
            return -1;
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
                // This will allow us to incorporate the holes into the sector outline without having problems where a previous
                // hole's connector is cutting through another hole that hasn't been incorporated yet.
                SortHolesByShortestConnectors(finishedHolesIDs);

                // Connect the remaining hole with the shortest connector into the sector's outline.
                AddHoleToSectorOutline(_SortedHolesList[0], finishedHolesIDs);
                _SortedHolesList.RemoveAt(0);

                // If all holes have been incorporated into the sector's outline, then break out of this loop.
                if (finishedHolesIDs.Count >= Holes.Count)
                    break;

            } // end for i


            return true;
        }

        /// <summary>
        /// Adds the first hole in the sorted list (the one with the shortest connector line segment) into the sector's outline.
        /// </summary>
        /// <returns>True if successful.</returns>
        private static bool AddHoleToSectorOutline(HoleData holeData, List<int> finishedHolesIDs)
        {
            // Get the closest connector data for this hole.
            ClosestConnectorData closestConnectorData = holeData.ClosestConnectorData;

            // Copy the hole's vertex list so we can invert it quick since we need it to be counterclockwise.
            List<Vector2> holeVerts = new List<Vector2>(holeData.Vertices);

            // Duplicate the first vertex at the end so that we connect the last vert to the first vert, before
            // then connecting back out to the sector outline.
            holeVerts.Add(holeVerts[0]);

            // Reverse the order of the hole vertices, so we go counterclockwise was we add its vertices into the sector's outline.
            holeVerts.Reverse();


            // Duplicate the vertex we are connecting to in the sector outline.
            int insertionIndex = closestConnectorData.SectorOutlinePointIndex;
            SectorOutline.Insert(closestConnectorData.SectorOutlinePointIndex, SectorOutline[closestConnectorData.SectorOutlinePointIndex]);

            // Insert the reversed hole vertices just after the two duplicated vertices we just made.
            insertionIndex++;
            SectorOutline.InsertRange(insertionIndex, holeVerts);


            // Add this hole's ID to the list of finished holes IDs.
            finishedHolesIDs.Add(holeData.ID);

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
                if (FindClosestConnector(curHoleData))
                {
                    // Insert this hole into the proper spot in the list based on the length of its shortest connector line segment.
                    for (int i = 0; i < _SortedHolesList.Count - 1; i++)
                    {
                        if (curHoleData.ClosestConnectorData.Length < _SortedHolesList[i].ClosestConnectorData.Length)
                        {
                            _SortedHolesList.Insert(i, curHoleData);
                            break;
                        }
                    }
                }


            } // end for i


            return true;
        }

        /// <summary>
        /// Finds the shortest line segment we can create to connect the hole outline to the sector's outline.
        /// </summary>
        /// <returns>True if successful.</returns>
        private static bool FindClosestConnector(HoleData holeData)
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
                        // Check if this line segment is a valid connector.
                        if (!IsValidConnector(holeData, i, j))
                        {
                            continue;
                        }
                        else
                        {
                            holeData.ClosestConnectorData.Length = distance;
                            holeData.ClosestConnectorData.HolePointIndex = i;
                            holeData.ClosestConnectorData.SectorOutlinePointIndex = j;
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
        /// <param name="holeID">The ID of the hole we are working on.</param>
        /// <param name="holeOutlinePointIndex">The end of the connector that is attached to the hole's outline.</param>
        /// <param name="sectorOutlinePointIndex">The end of the connector that is attached to the sector's outline.</param>
        /// <returns>True if successful.</returns>
        private static bool IsValidConnector(HoleData holeData, int holeOutlinePointIndex, int sectorOutlinePointIndex)
        {
            Vector2 connectorStart = holeData.Vertices[holeOutlinePointIndex];
            Vector2 connectorEnd = SectorOutline[sectorOutlinePointIndex];


            // Check if this line segment intersects with any line segments in any of the hole outlines.
            foreach (KeyValuePair<int, HoleData> pair in Holes)
            {
                HoleData curHole = pair.Value;

                Vector2 lineStart = Vector2.zero;
                Vector2 lineEnd = Vector2.zero;

                // If this is the hole we're checking, then skip it since
                if (curHole.ID == holeData.ID)
                    continue;


                // Iterate through all of the line segments in this hole's outline to see if this potential connector intersects with any.
                for (int j = 0; j < curHole.Vertices.Count; j++)
                {
                    lineStart = curHole.Vertices[j];
                    lineEnd = curHole.Vertices[Triangulator_Polygon.WrapIndex(j + 1, curHole.Vertices.Count)];

                    if (Lines.GetLineSegmentIntersectionPoint(connectorStart, connectorEnd, lineStart, lineEnd, out _, true))
                    {
                        Debug.LogError("Hole intersects with itself!");
                        return false;
                    }

                } // end for j

                // Iterate through all of the line segments in the sector's outline to see if this potential connector intersects with any.
                for (int k = 0; k < curHole.Vertices.Count; k++)
                {
                    lineStart = SectorOutline[k];
                    lineEnd = SectorOutline[Triangulator_Polygon.WrapIndex(k + 1, SectorOutline.Count)];

                    if (Lines.GetLineSegmentIntersectionPoint(connectorStart, connectorEnd, lineStart, lineEnd, out _, true))
                    {
                        Debug.LogError("Hole intersects with sector outline!");
                        return false;
                    }

                } // end for j

            } // end for i


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
