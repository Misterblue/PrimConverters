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
using System.Drawing;

using org.herbal3d.tools.SimplePromise;
using org.herbal3d.tools.AssetHandling;
using org.herbal3d.tools.Logging;

using OMV = OpenMetaverse;
using OMVR = OpenMetaverse.Rendering;

namespace org.herbal3d.tools.Converters
{
    public class PrimToMesh
    {
        OMVR.MeshmerizerR m_mesher;
        Logger m_log;
        String m_logHeader = "PrimToMesh:";

        public PrimToMesh() {
            m_mesher = new OMVR.MeshmerizerR();
            m_log = Logger.Instance();
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
            OMVR.FacetedMesh mesh = m_mesher.GenerateFacetedMesh(prim, lod);
            return mesh;
        }

        private OMVR.FacetedMesh MeshFromPrimSculptData(OMV.Primitive prim, IAssetFetcher assetFetcher, OMVR.DetailLevel lod) {
            OMVR.FacetedMesh ret = null;

            // Get the asset that the sculpty is built on
            EntityHandle texHandle = new EntityHandle(prim.Sculpt.SculptTexture);
            assetFetcher.FetchTexture(texHandle)
                .Then((bm) => {
                    ret = m_mesher.GenerateFacetedSculptMesh(prim, bm.Image.ExportBitmap(), lod);
                })
                .Rejected((e) => {
                    m_log.Error("{0} MeshFromPrimSculptData: Rejected FetchTexture: {1}: {2}", m_logHeader, texHandle, e);
                    ret = null;
                    throw e;
                });

            return ret;
        }

        private OMVR.FacetedMesh MeshFromPrimMeshData(OMV.Primitive prim, IAssetFetcher assetFetcher, OMVR.DetailLevel lod) {
            return null;
        }
    }
}
