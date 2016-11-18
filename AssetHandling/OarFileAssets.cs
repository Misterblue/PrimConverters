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

using OMV = OpenMetaverse;

using org.herbal3d.tools.SimplePromise;

namespace org.herbal3d.tools.AssetHandling {
    public class OarFileAssets : IAssetFetcher {

        private string assetDir;

        public OarFileAssets(string pAassetDir) {
            assetDir = pAassetDir;
        }

        public override SimplePromise<OMV.Assets.AssetTexture> FetchTexture(EntityHandle handle) {

            SimplePromise<OMV.Assets.AssetTexture> prom = new SimplePromise<OMV.Assets.AssetTexture>();

            return prom;
        }

        public override SimplePromise<byte[]> FetchRawAsset(EntityHandle handle) {

            SimplePromise<byte[]> prom = new SimplePromise<byte[]>();

            return prom;
        }

        public override void Dispose() {
            // nothing to do
        }
    }
}
