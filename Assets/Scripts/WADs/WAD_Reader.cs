using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using UnityEngine;

using DIY_DOOM.Maps;
using DIY_DOOM.WADs.Data;
using DIY_DOOM.WADs.Data.Maps;
using DIY_DOOM.WADs.Data.Textures;
using System.Runtime.InteropServices.WindowsRuntime;

namespace DIY_DOOM.WADs
{
    public class WAD_Reader
    {
        // BASIC FUNCTIONS FOR READING DATA FROM THE WAD FILE
        // ========================================================================================================================================================================================================

        public byte[] Read2Bytes(byte[] wadData, int offset)
        {
            byte[] bytes2 = new byte[2];

            Array.Copy(wadData, offset, bytes2, 0, 2);

            return bytes2;
        }

        public byte[] Read4Bytes(byte[] wadData, int offset)
        {
            byte[] bytes4 = new byte[4];

            Array.Copy(wadData, offset, bytes4, 0, 4);

            return bytes4;
        }

        public byte[] Read8Bytes(byte[] wadData, int offset)
        {
            byte[] bytes8 = new byte[8];

            Array.Copy(wadData, offset, bytes8, 0, 8);

            return bytes8;
        }

        public string Read8ByteString(byte[] wadData, int offset)
        {
            // The trim on the end is needed, as unused characters at the end of the lump name are all set to the null character. We need to remove those unnecessary chars from the end of the string.
            return Encoding.UTF8.GetString(Read8Bytes(wadData, offset)).Trim('\0');
        }



        // FUNCTIONS FOR READING WAD FILE STRUCTURE
        // ========================================================================================================================================================================================================
        
        public WAD_Header ReadHeaderData(byte[] wadData, int offset)
        {
            WAD_Header header = new WAD_Header();

            header.WAD_Type = System.Text.Encoding.UTF8.GetString(Read4Bytes(wadData, offset));
            header.DirectoryCount = BitConverter.ToUInt32(Read4Bytes(wadData, offset + 4));
            header.DirectoryOffset = BitConverter.ToUInt32(Read4Bytes(wadData, offset + 8));

            return header;
        }

        public WAD_DirectoryDef ReadDirectoryData(byte[] wadData, int offset)
        {
            WAD_DirectoryDef directory = new WAD_DirectoryDef();

            directory.LumpOffset = BitConverter.ToUInt32(Read4Bytes(wadData, offset));
            directory.LumpSize = BitConverter.ToUInt32(Read4Bytes(wadData, offset + 4));

            directory.LumpName = Read8ByteString(wadData, offset + 8);

            return directory;
        }



        // FUNCTIONS FOR READING LUMPS OF MAP DATA
        // ========================================================================================================================================================================================================

        public Vector3 ReadVertexData(byte[] wadData, int offset)
        {
            Vector3 vertex = MapUtils.Point2dTo3dXZ(BitConverter.ToInt16(Read2Bytes(wadData, offset)),
                                                    BitConverter.ToInt16(Read2Bytes(wadData, offset + 2)));

            return vertex;
        }

        public LineDef ReadLineDefData(byte[] wadData, int offset)
        {
            LineDef lineDef = new LineDef();

            lineDef.StartVertexID = BitConverter.ToUInt16(Read2Bytes(wadData, offset));
            lineDef.EndVertexID = BitConverter.ToUInt16(Read2Bytes(wadData, offset + 2));
            lineDef.Flags = (LineDefFlags)BitConverter.ToUInt16(Read2Bytes(wadData, offset + 4));
            lineDef.LineType = BitConverter.ToUInt16(Read2Bytes(wadData, offset + 6));
            lineDef.SectorTag = BitConverter.ToUInt16(Read2Bytes(wadData, offset + 8));
            
            lineDef.RightSideDef = BitConverter.ToInt16(Read2Bytes(wadData, offset + 10));
            lineDef.LeftSideDef = BitConverter.ToInt16(Read2Bytes(wadData, offset + 12));

            return lineDef;
        }

        public ThingDef ReadThingData(byte[] wadData, int offset)
        {
            ThingDef thing = new ThingDef();
            
            float x = BitConverter.ToInt16(Read2Bytes(wadData, offset));
            float y = BitConverter.ToInt16(Read2Bytes(wadData, offset + 2));
            thing.Position = MapUtils.Point2dTo3dXZ(x, y);

            thing.Angle = BitConverter.ToUInt16(Read2Bytes(wadData, offset + 4));
            thing.Type = BitConverter.ToUInt16(Read2Bytes(wadData, offset + 6));
            thing.Flags = BitConverter.ToUInt16(Read2Bytes(wadData, offset + 8));

            return thing;
        }

        public NodeDef ReadNodeData(byte[] wadData, int offset)
        {
            NodeDef node = new NodeDef();

            float x = BitConverter.ToInt16(Read2Bytes(wadData, offset));
            float y = BitConverter.ToInt16(Read2Bytes(wadData, offset + 2));
            node.PartitionStart = MapUtils.Point2dTo3dXZ(x, y);
            
            x = BitConverter.ToInt16(Read2Bytes(wadData, offset + 4));
            y = BitConverter.ToInt16(Read2Bytes(wadData, offset + 6));
            node.DeltaToPartitionEnd = MapUtils.Point2dTo3dXZ(x, y);
            node.PartitionEnd = node.PartitionStart + node.DeltaToPartitionEnd;

            int RightBox_Top = BitConverter.ToInt16(Read2Bytes(wadData, offset + 8));
            int RightBox_Bottom = BitConverter.ToInt16(Read2Bytes(wadData, offset + 10));
            int RightBox_Left = BitConverter.ToInt16(Read2Bytes(wadData, offset + 12));
            int RightBox_Right = BitConverter.ToInt16(Read2Bytes(wadData, offset + 14));

            node.RightBox_BottomLeft = MapUtils.Point2dTo3dXZ(RightBox_Left, RightBox_Bottom);
            node.RightBox_TopRight = MapUtils.Point2dTo3dXZ(RightBox_Right, RightBox_Top);

            int LeftBox_Top = BitConverter.ToInt16(Read2Bytes(wadData, offset + 16));
            int LeftBox_Bottom = BitConverter.ToInt16(Read2Bytes(wadData, offset + 18));
            int LeftBox_Left = BitConverter.ToInt16(Read2Bytes(wadData, offset + 20));
            int LeftBox_Right = BitConverter.ToInt16(Read2Bytes(wadData, offset + 22));

            node.LeftBox_BottomLeft = MapUtils.Point2dTo3dXZ(LeftBox_Left, LeftBox_Bottom);
            node.LeftBox_TopRight = MapUtils.Point2dTo3dXZ(LeftBox_Right, LeftBox_Top);


            node.RightChildID = BitConverter.ToUInt16(Read2Bytes(wadData, offset + 24));
            node.LeftChildID = BitConverter.ToUInt16(Read2Bytes(wadData, offset + 26));

            return node;
        }

        public SubSectorDef ReadSubSectorData(byte[] wadData, int offset)
        {
            SubSectorDef subSector = new SubSectorDef();

            subSector.SegCount = BitConverter.ToUInt16(Read2Bytes(wadData, offset));
            subSector.FirstSegID = BitConverter.ToUInt16(Read2Bytes(wadData, offset + 2));
            
            return subSector;
        }

        public SegDef ReadSegData(byte[] wadData, int offset)
        {
            SegDef seg = new SegDef();

            seg.StartVertexID = BitConverter.ToUInt16(Read2Bytes(wadData, offset));
            seg.EndVertexID = BitConverter.ToUInt16(Read2Bytes(wadData, offset + 2));
            
            seg.Angle = BitConverter.ToInt16(Read2Bytes(wadData, offset + 4));
            
            seg.LineDefID = BitConverter.ToUInt16(Read2Bytes(wadData, offset + 6));
            seg.Direction = BitConverter.ToUInt16(Read2Bytes(wadData, offset + 8));
            seg.Offset = BitConverter.ToUInt16(Read2Bytes(wadData, offset + 10));

            return seg;
        }

        public Palette ReadPaletteData(byte[] wadData, int offset)
        {
            Color32[] colors = new Color32[256];

            for (int i = 0; i < 256; i++)
            {
                colors[i] = new Color32(wadData[offset++], 
                                        wadData[offset++], 
                                        wadData[offset++],
                                        255); // Full alpha.
            }


            Palette palette = new Palette(colors);

            return palette;
        }

        public PatchHeader ReadPatchHeader(byte[] wadData, int offset) 
        { 
            PatchHeader patchHeader = new PatchHeader();

            patchHeader.Width = BitConverter.ToUInt16(Read2Bytes(wadData, offset));
            patchHeader.Height = BitConverter.ToUInt16(Read2Bytes(wadData, offset + 2));

            patchHeader.X_Offset = BitConverter.ToInt16(Read2Bytes(wadData, offset + 4));
            patchHeader.Y_Offset = BitConverter.ToInt16(Read2Bytes(wadData, offset + 6));


            uint[] columnOffsets = new uint[patchHeader.Width];

            offset += 8;
            for (int i = 0; i < patchHeader.Width; i++)
            {
                columnOffsets[i] = BitConverter.ToUInt32(Read4Bytes(wadData, offset));
                offset += 4;
            }

            patchHeader.SetColumnOffsets(columnOffsets);

            //patchHeader.DEBUG_Print(false);
            
            return patchHeader;
        }

        public PatchColumn ReadPatchColumn(byte[] wadData, int offset, out int nextColumnOffset)
        {
            PatchColumn patchColumn = new PatchColumn();

            patchColumn.TopDelta = wadData[offset++];
            if (patchColumn.TopDelta != 0xFF)
            {
                patchColumn.Length = wadData[offset++];
                patchColumn.PaddingPre = wadData[offset++];

                byte[] columnData = new byte[patchColumn.Length];
                for (int i = 0; i < patchColumn.Length; i++)
                {
                    columnData[i] = wadData[offset++];
                }

                patchColumn.PaddingPost = wadData[offset++];

                patchColumn.SetColumnData(columnData);

                //patchColumn.DEBUG_Print();
            }


            nextColumnOffset = offset;

            return patchColumn;
        }

        public PatchNamesHeader ReadPatchNamesHeader(byte[] wadData, int offset)
        {
            PatchNamesHeader patchNamesHeader = new PatchNamesHeader();

            patchNamesHeader.PatchNamesCount = BitConverter.ToUInt32(Read4Bytes(wadData, offset));
            
            patchNamesHeader.PatchNamesOffset = (uint) offset + 4;

            return patchNamesHeader;
        }

        public TextureHeader ReadTextureHeader(byte[] wadData, int offset)
        {
            TextureHeader textureHeader = new TextureHeader();

            textureHeader.TexturesCount = BitConverter.ToUInt32(Read4Bytes(wadData, offset));
            textureHeader.TexturesOffset = BitConverter.ToUInt32(Read4Bytes(wadData, offset + 4));


            offset += 4;
            for (int i = 0; i < textureHeader.TexturesCount; i++)
            {
                textureHeader.AddTextureDataOffset(BitConverter.ToUInt32(Read4Bytes(wadData, offset)));
                offset += 4;
            }


            return textureHeader;
        }

        public TextureData ReadTextureData(byte[] wadData, int offset)
        {
            TextureData textureData = new TextureData();

            textureData.TextureName = Read8ByteString(wadData, offset);

            textureData.Flags = BitConverter.ToUInt16(Read2Bytes(wadData, offset + 8));
            textureData.Width = BitConverter.ToUInt16(Read2Bytes(wadData, offset + 12));
            textureData.Height = BitConverter.ToUInt16(Read2Bytes(wadData, offset + 14));
            
            textureData.ColumnDirectory = BitConverter.ToUInt32(Read4Bytes(wadData, offset + 16));
            
            textureData.PatchCount = BitConverter.ToUInt16(Read2Bytes(wadData, offset + 20));
            
            
            offset += 22;
            for (int i = 0; i < textureData.PatchCount; i++)
            {
                textureData.AddTexturePatch(ReadTexturePatch(wadData, offset));
                offset += 10;
            }


            return textureData;
        }

        public TexturePatch ReadTexturePatch(byte[] wadData, int offset)
        {
            TexturePatch texturePatch = new TexturePatch();

            texturePatch.X_Offset = BitConverter.ToInt16(Read2Bytes(wadData, offset));
            texturePatch.Y_Offset = BitConverter.ToInt16(Read2Bytes(wadData, offset + 2));
            texturePatch.PatchNameIndex = BitConverter.ToUInt16(Read2Bytes(wadData, offset + 4));
            texturePatch.StepDir = BitConverter.ToUInt16(Read2Bytes(wadData, offset + 6));
            texturePatch.ColorMap = BitConverter.ToUInt16(Read2Bytes(wadData, offset + 8));

            return texturePatch;
        }
    }
}