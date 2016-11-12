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
using System.Text;

using org.herbal3d.tools.Logging;
using org.herbal3d.tools.PrimConverters;
using org.herbal3d.tools.AssetHandling;
using org.herbal3d.tools.SimplePromise;

using OMV = OpenMetaverse;
using OMVA = OpenMetaverse.Assets;
using OMVR = OpenMetaverse.Rendering;

using OpenSim.Region.Framework.Scenes;
using OpenSim.Region.Framework.Scenes.Serialization;

namespace org.herbal3d.tools.Converters {
    public class ExtendedPrim {
        public OMVA.PrimObject primObject;
        public OMV.Primitive primitive;
        public OMVR.FacetedMesh facetedMesh;
    }

    public static class Converters {

        static Logger m_log = Logger.Instance();

        // This doesn't work on new OAR objects as the OAR file deserializer in
        //    libopenmetavers is very old and fails to parse objects.
        // See DoOnePrimToMesh below for an implementation using the OpenSim serializers
        public static void DoToMesh(Stream inFile, Stream outFile, string assetDir) {
            String xmlString;

            // Flag saying to merge linksets into a single mesh
            bool shouldMerge = Globals.Params.ContainsKey("--merge");

            using (StreamReader inn = new StreamReader(inFile, Encoding.UTF8)) {
                xmlString = inn.ReadToEnd();
            }

            // Parse and convert the XML
            OMVA.AssetPrim primAsset = new OMVA.AssetPrim(xmlString);

            PrimToMesh assetMesher = new PrimToMesh();

            // Convert the object definition into a mesh

            CreateAllMeshesInSOP(primAsset, assetMesher, assetDir)
                .Then(extendedAssetList => {
                    
                })
                .Rejected(e => {
                    m_log.Error("Failed mesh extraction: " + e);
                });
        }

        // Fetch all the meshes for this SceneObjectGroup.
        // The problem is that the different pieces may take different times to fetch (get the assets)
        private static SimplePromise<List<ExtendedPrim>> CreateAllMeshesInSOP(OMVA.AssetPrim primAsset, PrimToMesh assetMesher, string assetDir) {
            SimplePromise<List<ExtendedPrim>> prom = new SimplePromise<List<ExtendedPrim>>();

            List<ExtendedPrim> meshes = new List<ExtendedPrim>();

            using (IAssetFetcher assetFetcher = new OarFileAssets(assetDir)) {

                // fetch the mesh for the root and the children
                int totalChildren = primAsset.Children.Count;
                foreach (OMVA.PrimObject onePrimObject in primAsset.Children) {
                    m_log.Debug("CreateAllMeshesInSOP: foreach onePrimObject: {0}", onePrimObject.ID);
                    OMV.Primitive aPrim = onePrimObject.ToPrimitive();
                    assetMesher.CreateMeshResource(aPrim, assetFetcher, OMVR.DetailLevel.Highest)
                        .Then(facetedMesh => {
                            lock (meshes) {
                                m_log.Debug("CreateAllMeshesInSOP: foreach onePrimObject: {0}, primAsset={1}, fmesh={2}",
                                                        onePrimObject.ID, aPrim.ID, facetedMesh.Faces.Count);
                                ExtendedPrim ePrim = new ExtendedPrim();
                                ePrim.primitive = aPrim;
                                ePrim.primObject = onePrimObject;
                                ePrim.facetedMesh = facetedMesh;
                                meshes.Add(ePrim);
                            }
                            if (--totalChildren <= 0) {
                                prom.Resolve(meshes);
                            }
                        })
                        .Rejected(e => {
                            prom.Reject(e);
                        });
                }
            }
            return prom;
        }

        // Fetch all the meshes for this SceneObjectGroup.
        // The problem is that the different pieces may take different times to fetch (get the assets)
        private static SimplePromise<List<ExtendedPrim>> CreateAllMeshesInSOP(SceneObjectGroup sog, PrimToMesh assetMesher, string assetDir) {
            SimplePromise<List<ExtendedPrim>> prom = new SimplePromise<List<ExtendedPrim>>();

            List<ExtendedPrim> meshes = new List<ExtendedPrim>();

            using (IAssetFetcher assetFetcher = new OarFileAssets(assetDir)) {

                // fetch the mesh for the root and the children
                int totalChildren = sog.Parts.GetLength(0);
                foreach (SceneObjectPart onePrimObject in sog.Parts) {
                    m_log.Debug("CreateAllMeshesInSOP: foreach onePrimObject: {0}", onePrimObject.UUID);
                    OMV.Primitive aPrim = onePrimObject.Shape;
                    assetMesher.CreateMeshResource(aPrim, assetFetcher, OMVR.DetailLevel.Highest)
                        .Then(facetedMesh => {
                            lock (meshes) {
                                m_log.Debug("CreateAllMeshesInSOP: foreach onePrimObject: {0}, primAsset={1}, fmesh={2}",
                                                        onePrimObject.UUID, aPrim.ID, facetedMesh.Faces.Count);
                                ExtendedPrim ePrim = new ExtendedPrim();
                                ePrim.primitive = aPrim;
                                ePrim.primObject = onePrimObject;
                                ePrim.facetedMesh = facetedMesh;
                                meshes.Add(ePrim);
                            }
                            if (--totalChildren <= 0) {
                                prom.Resolve(meshes);
                            }
                        })
                        .Rejected(e => {
                            prom.Reject(e);
                        });
                }
            }
            return prom;
        }
        // This reads one serialized SceneObjectGroup using the OpenSim sserializer.
        public static void OnePrimToMesh(Stream inFile, Stream outFile, string assetDir) {
            String xmlString;

            // Flag saying to merge linksets into a single mesh
            bool shouldMerge = Globals.Params.ContainsKey("--merge");

            using (StreamReader inn = new StreamReader(inFile, Encoding.UTF8)) {
                xmlString = inn.ReadToEnd();
            }

            // Parse and convert the XML
            SceneObjectGroup sog = SceneObjectSerializer.FromXml2Format(xmlString);

            PrimToMesh assetMesher = new PrimToMesh();

            // Convert the object definition into a mesh

            CreateAllMeshesInSOP(sog, assetMesher, assetDir)
                .Then(extendedAssetList => {
                    
                })
                .Rejected(e => {
                    m_log.Error("Failed mesh extraction: " + e);
                });
        }


        public static void DoToPNG(Stream inFile, Stream outFile) {
            // Read the input file into a byte array
            MemoryStream ms = new MemoryStream();
            inFile.CopyTo(ms);

            // Convert from JPEG2000 to an image
            Image tempImage = CSJ2K.J2kImage.FromBytes(ms.ToArray());
            try {
                using (Bitmap textureBitmap = new Bitmap(tempImage.Width, tempImage.Height,
                            System.Drawing.Imaging.PixelFormat.Format32bppArgb)) {
                    // convert the raw image into a channeled image
                    using (Graphics graphics = Graphics.FromImage(textureBitmap)) {
                        graphics.DrawImage(tempImage, 0, 0);
                        graphics.Flush();
                    }
                    // Write out the converted image as PNG
                    textureBitmap.Save(outFile, System.Drawing.Imaging.ImageFormat.Png);
                    outFile.Flush();
                }
            }
            catch (Exception e) {
                m_log.Error("FAILED PNG FILE CREATION: {0}", e);
                throw e;
            }
        }
    }
}
