using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using OMV = OpenMetaverse;
using OMVR = OpenMetaverse.Rendering;

namespace AssetHandling {

    class AssetUtil {

        // Routine from OpenSim which creates a hash for the prim shape
        public static ulong GetMeshKey(OMV.Primitive prim, OMV.Vector3 size, float lod) {
            // Don't use the libOpenMetaverse code since it doesn't account for meshes and scale.
            // ulong hash = (ulong)prim.GetHashCode();

            ulong hash = 5381;

            hash = djb2(hash, (byte)prim.PrimData.PathCurve);
            hash = djb2(hash, prim.PrimData.ProfileHollow);
            hash = djb2(hash, (byte)prim.PrimData.ProfileHole);
            hash = djb2(hash, prim.PrimData.PathBegin);
            hash = djb2(hash, prim.PrimData.PathEnd);
            hash = djb2(hash, prim.PrimData.PathScaleX);
            hash = djb2(hash, prim.PrimData.PathScaleY);
            hash = djb2(hash, prim.PrimData.PathShearX);
            hash = djb2(hash, prim.PrimData.PathShearY);
            hash = djb2(hash, (byte)prim.PrimData.PathTwist);
            hash = djb2(hash, (byte)prim.PrimData.PathTwistBegin);
            hash = djb2(hash, (byte)prim.PrimData.PathRadiusOffset);
            hash = djb2(hash, (byte)prim.PrimData.PathTaperX);
            hash = djb2(hash, (byte)prim.PrimData.PathTaperY);
            hash = djb2(hash, prim.PrimData.PathRevolutions);
            hash = djb2(hash, (byte)prim.PrimData.PathSkew);
            hash = djb2(hash, prim.PrimData.ProfileBegin);
            hash = djb2(hash, prim.PrimData.ProfileEnd);
            hash = djb2(hash, prim.PrimData.ProfileHollow);

            // TODO: Separate scale out from the primitive shape data (after
            // scaling is supported at the physics engine level)
            // byte[] scaleBytes = size.GetBytes();
            // for (int i = 0; i < scaleBytes.Length; i++)
            //     hash = djb2(hash, scaleBytes[i]);
            hash = djb2(hash, size.X);
            hash = djb2(hash, size.Y);
            hash = djb2(hash, size.Z);

            // Include LOD in hash, accounting for endianness
            byte[] lodBytes = new byte[4];
            Buffer.BlockCopy(BitConverter.GetBytes(lod), 0, lodBytes, 0, 4);
            if (!BitConverter.IsLittleEndian) {
                Array.Reverse(lodBytes, 0, 4);
            }
            for (int i = 0; i < lodBytes.Length; i++)
                hash = djb2(hash, lodBytes[i]);

            // include sculpt/mesh UUID
            if (prim.Sculpt != null) {
                byte[] scaleBytes = prim.Sculpt.SculptTexture.GetBytes();
                for (int i = 0; i < scaleBytes.Length; i++)
                    hash = djb2(hash, scaleBytes[i]);
            }

            // since these are displayed meshes, we need to include the material
            // information
            if (prim.Textures != null) {
                for (uint ii = 0; ii < 7; ii++) {
                    OMV.Primitive.TextureEntryFace texFace = prim.Textures.GetFace(ii);
                    if (texFace != null) {
                        hash = djb2(hash, texFace.RGBA.R);
                        hash = djb2(hash, texFace.RGBA.G);
                        hash = djb2(hash, texFace.RGBA.B);
                        hash = djb2(hash, texFace.RGBA.A);
                        hash = djb2(hash, texFace.RepeatU);
                        hash = djb2(hash, texFace.RepeatV);
                        hash = djb2(hash, texFace.OffsetU);
                        hash = djb2(hash, texFace.OffsetV);
                        hash = djb2(hash, texFace.Rotation);
                        hash = djb2(hash, texFace.Glow);
                        hash = djb2(hash, (byte)texFace.Bump);
                        hash = djb2(hash, (byte)texFace.Shiny);
                        hash = djb2(hash, texFace.Fullbright ? 1.0f : 0.5f);
                        hash = djb2(hash, texFace.Glow);
                        byte[] texIDBytes = texFace.TextureID.GetBytes();
                        for (int jj = 0; jj < texIDBytes.Length; jj++) {
                            hash = djb2(hash, texIDBytes[jj]);
                        }
                    }
                }
            }

            return hash;
        }

        private static ulong djb2(ulong hash, byte c) {
            return ((hash << 5) + hash) + (ulong)c;
        }

        private static ulong djb2(ulong hash, ushort c) {
            hash = ((hash << 5) + hash) + (ulong)((byte)c);
            return ((hash << 5) + hash) + (ulong)(c >> 8);
        }

        private static ulong djb2(ulong hash, float c) {
            byte[] asBytes = BitConverter.GetBytes(c);
            hash = ((hash << 5) + hash) + (ulong)asBytes[0];
            hash = ((hash << 5) + hash) + (ulong)asBytes[1];
            hash = ((hash << 5) + hash) + (ulong)asBytes[2];
            return ((hash << 5) + hash) + (ulong)asBytes[3];
        }
    }
}
