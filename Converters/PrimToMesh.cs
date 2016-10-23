/* ==============================================================================
Copyright (c) 2016 Robert Adams

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
================================================================================ */

using System;

using org.herbal3d.tools.SimplePromise;
using org.herbal3d.tools.AssetHandling;

using OMV = OpenMetaverse;
using OMVR = OpenMetaverse.Rendering;

namespace org.herbal3d.tools.Converters
{
    public class PrimToMesh
    {

        public PrimToMesh() {
        }

        /// <summary>
        /// Create and return a faceted mesh.
        /// </summary>
        public SimplePromise<OMVR.FacetedMesh> CreateMeshResource(OMV.Primitive prim, IAssetFetcher assetFetcher, OMVR.DetailLevel lod) {

            SimplePromise<OMVR.FacetedMesh> prom = new SimplePromise<OMVR.FacetedMesh>();

            OMVR.FacetedMesh mesh;
            try {
                if (prim.Sculpt != null) {
                    if (prim.Sculpt.Type == OMV.SculptType.Mesh) {
                        mesh = MeshFromPrimMeshData(prim, assetFetcher, lod);
                        prom.Resolve(mesh);
                    }
                    else {
                        mesh = MeshFromPrimSculptData(prim, assetFetcher, lod);
                        prom.Resolve(mesh);
                    }
                }
                else {
                    mesh = MeshFromPrimShapeData(prim, lod);
                    prom.Resolve(mesh);
                }
            }
            catch (Exception e) {
                prom.Reject(e);
            }

            return prom;
        }

        private OMVR.FacetedMesh MeshFromPrimShapeData(OMV.Primitive prim, OMVR.DetailLevel lod) {
            return null;
        }

        private OMVR.FacetedMesh MeshFromPrimSculptData(OMV.Primitive prim, IAssetFetcher assetFetcher, OMVR.DetailLevel lod) {
            return null;
        }

        private OMVR.FacetedMesh MeshFromPrimMeshData(OMV.Primitive prim, IAssetFetcher assetFetcher, OMVR.DetailLevel lod) {
            return null;
        }
    }
}
