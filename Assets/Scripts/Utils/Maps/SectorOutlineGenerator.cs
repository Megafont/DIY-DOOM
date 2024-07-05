using DIY_DOOM.Maps;
using DIY_DOOM.WADs.Data.Maps;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;



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
        /// This list holds a list of segs for each hole that exists within this sector (aka another sector)
        /// </summary>
        private static List<List<Vector2>> Holes = new List<List<Vector2>>();

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

            if (Holes.Count < 1)
            {
                Debug.LogWarning("No complete outlines were found for this sector!");
                return;
            }


            // Find the largest outline. This one is the exterior walls of the sector.
            int index = FindLargestOutline();
            if (index < 0)
            {
                Debug.LogError("Failed to find the largest outline for this sector!");
                return;
            }

            Debug.Log($"Largest: {index}    {Holes[index].Count}");

            // Copy the largest outline into SectorOutline, and remove it from the holes list.
            _SectorDef.SectorOutline = Holes[index];
            Holes.RemoveAt(index);
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
            // TODO: Figure out why discarding bad triangles doesn't seem to be working quite right.
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

                Holes.Add(new List<Vector2>());
                Holes[holeIndex].Add(MapUtils.Point3dTo2d(firstSegStartPoint));
                Holes[holeIndex].Add(MapUtils.Point3dTo2d(lastSegEndPoint));


                bool curOutlineIsDone = false;
                int failedOutlineAttempts = 0;
                while (!curOutlineIsDone)
                {
                    curOutlineIsDone = false;
                    int nextIndex = FindNextSeg(allSectorSegs, holeIndex, firstSegStartPoint, ref lastSegEndPoint);

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
                if (Holes[holeIndex].Count < 3)
                {
                    Holes.RemoveAt(holeIndex);
                    holeIndex--;

                    consecutiveFailedHoleOutlineAttempts++;
                    invalidOutlines++;
                }

                if (allSectorSegs.Count <= 0)
                    break;


                if (consecutiveFailedHoleOutlineAttempts >= MAX_OUTLINE_ATTEMPTS / 2)
                {
                    Debug.Log("BREAK (OUTER)");
                    break;
                }
            } // end while


            Debug.Log($"Invalid Outlines Removed: {invalidOutlines}");

        }

        /// <summary>
        /// This function goes through all of the outlines we found for this sector and its holes, and finds the biggest one (the exterior walls of the sector).
        /// </summary>
        private static int FindLargestOutline()
        {
            List<Vector2> minExtents = new List<Vector2>();
            List<Vector2> maxExtents = new List<Vector2>();


            // Iterate through all the outlines
            for (int i = 0; i < Holes.Count; i++)
            {
                Vector2 curMin = new Vector2(float.MaxValue, float.MaxValue);
                Vector2 curMax = new Vector2(float.MinValue, float.MinValue);
                
                Debug.Log($"HOLE[{i}]");

                // Iterate through all of the vertices of this outline
                for (int j = 0; j < Holes[i].Count; j++)
                {
                    Vector2 vert = Holes[i][j];

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

            Debug.Log($"IndexOfLargestArea:    {indexOfLargest}");

            return indexOfLargest;
        }

        private static int FindNextSeg(List<SegDef> segs, int holeIndex, Vector3 firstSegStartPoint, ref Vector3 lastSegEndPoint)
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
                        Holes[holeIndex].Add(MapUtils.Point3dTo2d(vertToAdd));
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

    }
}
