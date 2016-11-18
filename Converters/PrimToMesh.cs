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
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;

using org.herbal3d.tools.SimplePromise;
using org.herbal3d.tools.AssetHandling;
using org.herbal3d.tools.Logging;

using OMV = OpenMetaverse;
using OMVR = OpenMetaverse.Rendering;
using OpenMetaverse.StructuredData;

namespace org.herbal3d.tools.Converters
{
    public class PrimToMesh : IDisposable
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
        public SimplePromise<ExtendedPrimGroup> CreateMeshResource(OMV.Primitive prim, IAssetFetcher assetFetcher, OMVR.DetailLevel lod) {

            SimplePromise<ExtendedPrimGroup> prom = new SimplePromise<ExtendedPrimGroup>();

            ExtendedPrimGroup mesh;
            try {
                if (prim.Sculpt != null) {
                    if (prim.Sculpt.Type == OMV.SculptType.Mesh) {
                        m_log.Debug("CreateMeshResource: creating mesh");
                        MeshFromPrimMeshData(prim, assetFetcher, lod)
                            .Then(fm => {
                                prom.Resolve(fm);
                            })
                            .Rejected(e => {
                                prom.Reject(e);
                            });
                    }
                    else {
                        m_log.Debug("CreateMeshResource: creating sculpty");
                        MeshFromPrimSculptData(prim, assetFetcher, lod)
                            .Then(fm => {
                                prom.Resolve(fm);
                            })
                            .Rejected(e => {
                                prom.Reject(e);
                            });
                    }
                }
                else {
                    m_log.Debug("CreateMeshResource: creating primshape");
                    mesh = MeshFromPrimShapeData(prim, lod);
                    prom.Resolve(mesh);
                }
            }
            catch (Exception e) {
                prom.Reject(e);
            }

            return prom;
        }

        private ExtendedPrimGroup MeshFromPrimShapeData(OMV.Primitive prim, OMVR.DetailLevel lod) {
            OMVR.FacetedMesh mesh = m_mesher.GenerateFacetedMesh(prim, lod);

            ExtendedPrim extPrim = new ExtendedPrim();
            extPrim.facetedMesh = mesh;
            extPrim.primitive = prim;

            ExtendedPrimGroup extPrimGroup = new ExtendedPrimGroup();
            // TODO?? how to add SOG for back reference?
            extPrimGroup.Add(PrimGroupType.lod1, extPrim);

            return extPrimGroup;
        }

        private SimplePromise<ExtendedPrimGroup> MeshFromPrimSculptData(OMV.Primitive prim, IAssetFetcher assetFetcher, OMVR.DetailLevel lod) {
            SimplePromise<ExtendedPrimGroup> prom = new SimplePromise<ExtendedPrimGroup>();

            // Get the asset that the sculpty is built on
            EntityHandle texHandle = new EntityHandle(prim.Sculpt.SculptTexture);
            assetFetcher.FetchTexture(texHandle)
                .Then((bm) => {
                    OMVR.FacetedMesh fMesh = m_mesher.GenerateFacetedSculptMesh(prim, bm.Image.ExportBitmap(), lod);

                    ExtendedPrim extPrim = new ExtendedPrim();
                    extPrim.facetedMesh = fMesh;
                    extPrim.primitive = prim;

                    ExtendedPrimGroup extPrimGroup = new ExtendedPrimGroup();
                    // TODO?? how to add SOG for back reference?
                    extPrimGroup.Add(PrimGroupType.lod1, extPrim);

                    prom.Resolve(extPrimGroup);
                })
                .Rejected((e) => {
                    m_log.Error("{0} MeshFromPrimSculptData: Rejected FetchTexture: {1}: {2}", m_logHeader, texHandle, e);
                    prom.Reject(e);
                });

            return prom;
        }

        private SimplePromise<ExtendedPrimGroup> MeshFromPrimMeshData(OMV.Primitive prim, IAssetFetcher assetFetcher, OMVR.DetailLevel lod) {
            SimplePromise<ExtendedPrimGroup> prom = new SimplePromise<ExtendedPrimGroup>();

            // Get the asset that the mesh is built on
            EntityHandle meshHandle = new EntityHandle(prim.Sculpt.SculptTexture);
            try {
                assetFetcher.FetchRawAsset(meshHandle)
                    .Then((bm) => {
                        ExtendedPrimGroup extPrimGroup = UnpackMeshData(prim, bm);
                        prom.Resolve(extPrimGroup);
                    })
                    .Rejected((e) => {
                        m_log.Error("{0} MeshFromPrimSculptData: Rejected FetchTexture: {1}", m_logHeader, e);
                        prom.Reject(e);
                    });
            }
            catch (Exception e) {
                prom.Reject(e);
            }

            return prom;
        }

        // =========================================================
        public ExtendedPrimGroup UnpackMeshData(OMV.Primitive prim, byte[] rawMeshData) {
            ExtendedPrimGroup subMeshes = new ExtendedPrimGroup();

            OSDMap meshOsd = new OSDMap();
            List<PrimMesher.Coord> coords = new List<PrimMesher.Coord>();
            List<PrimMesher.Face> faces = new List<PrimMesher.Face>();

            long start = 0;
            using (MemoryStream data = new MemoryStream(rawMeshData)) {
                try {
                    OSD osd = OSDParser.DeserializeLLSDBinary(rawMeshData);
                    if (osd is OSDMap)
                        meshOsd = (OSDMap)osd;
                    else {
                        throw new Exception("UnpackMeshData: parsing mesh data did not return an OSDMap");
                    }
                }
                catch (Exception e) {
                    m_log.Error("UnpackMeshData: Exception deserializing mesh asset header:" + e.ToString());
                }
                start = data.Position;
            }


            Dictionary<String, String> lodSections = new Dictionary<string, string>() {
                {"high_lod", "lod1" },
                {"medium_lod", "lod2" },
                {"low_lod", "lod3" },
                {"lowest_lod", "lod4" },
                {"physics_shape", "lod4" },
                {"physics_mesh", "lod4" },
                {"physics_convex", "lod4" },
            };

            foreach (KeyValuePair<string,string> lodSection in lodSections) {
            }

            return subMeshes;
        }

        // =========================================================
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~PrimToMesh() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose() {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
