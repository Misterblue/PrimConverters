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
using System.IO;

using org.herbal3d.tools.ParameterParsing;
using org.herbal3d.tools.Logging;
using org.herbal3d.tools.Converters;

namespace org.herbal3d.tools.PrimConverters {
    class PrimConverters {

        Dictionary<string, string> m_Parameters;

        public const string pOp = "--op";
        public const string pInFile = "--input";
        public const string pOutFile = "--output";
        public const string pAssetDir = "--assetdir";

        Logger m_log;

        private string Invocation() {
            return @"Invocation:
PrimConverters op opParameters
        where 'op' must be one of:
";
        }

        static void Main(string[] args) {
            PrimConverters prog = new PrimConverters();
            prog.Start(args);
            return;
        }

        public PrimConverters() {
            m_log = Logger.Instance();
            m_log.LogLevel = Logger.LOGLEVEL.NONE;
        }

        public void Start(string[] args) {
            m_Parameters = ParameterParse.ParseArguments(args, true /* firstOpFlag */, false /* multipleFiles */);
            foreach (KeyValuePair<string, string> kvp in m_Parameters) {
                string key = kvp.Key.ToLower();
                switch (kvp.Key) {
                    case ParameterParse.FIRST_PARAM:
                        Globals.Params[pOp]= kvp.Value.ToLower();
                        break;
                    case "-v":
                    case "--verbose":
                        m_log.LogLevel = Logger.LOGLEVEL.INFO;
                        break;
                    case "--debug":
                        m_log.LogLevel = Logger.LOGLEVEL.DEBUG;
                        break;
                    case ParameterParse.ERROR_PARAM:
                        // if we get here, the parser found an error
                        m_log.Error("Parameter error: " + kvp.Value);
                        m_log.Error(Invocation());
                        return;
                    default:
                        if (key.Equals("-i")) key = pInFile;
                        if (key.Equals("-o")) key = pOutFile;
                        Globals.Params[key] = kvp.Value;
                        break;
                }
            }

            // Verify parameters
            if (!Globals.Params.ContainsKey(pOp)) {
                m_log.Error("ERROR: An operation must be specified");
                m_log.Error(Invocation());
                return;
            }

            // Do the requested conversion
            switch (Globals.Params[pOp]) {
                case "tomesh":
                    DoToMesh();
                    break;
                case "topng":
                    DoToPNG();
                    break;
                default:
                    break;
            }
        }

        // Read a OAR extracted file and output a mesh.
        // Parameters are the input file and the output file.
        private void DoToMesh() {
            string inFile;
            string outFile;
            string assetDir;
            if (!Globals.Params.TryGetValue(pInFile, out inFile)) {
                m_log.Error("ERROR: An input file must be specified");
                m_log.Error(ToMeshInvocation());
                return;
            }

            if (!Globals.Params.TryGetValue(pOutFile, out outFile)) {
                m_log.Error("ERROR: An output file must be specified");
                m_log.Error(ToMeshInvocation());
                return;
            }

            if (!Globals.Params.TryGetValue(pAssetDir, out assetDir)) {
                m_log.Error("ERROR: An asset directory must be specified");
                m_log.Error(ToMeshInvocation());
                return;
            }

            try {
                using (Stream inFileStream = new FileStream(inFile, FileMode.Open, FileAccess.Read)) {
                    using (Stream outFileStream = new FileStream(outFile, FileMode.Create, FileAccess.Write)) {
                        Converters.Converters.DoToMesh(inFileStream, outFileStream, assetDir);
                    }
                }
            }
            catch (Exception e) {
                m_log.Error("ERROR: Could not use input/output files: " + e);
            }
        }

        private string ToMeshInvocation() {
            return @"ToMesh converts an OAR extracted object definition into a mesh file
INVOCATION:
PrimConverters ToMesh
        -i|--input inputFilename
        -o|--output outputFilename
        --format {RAW|OBJ} (default 'RAW')
        -v|--verbose
";
        }

        // Read a JPEG2000 texture file and output a PNG formatted version
        // Parameters are the input file and the output file.
        private void DoToPNG() {
            string inFile;
            string outFile;

            if (!Globals.Params.TryGetValue(pInFile, out inFile)) {
                m_log.Error("ERROR: An input file must be specified");
                m_log.Error(ToPNGInvocation());
                return;
            }

            if (!Globals.Params.TryGetValue(pOutFile, out outFile)) {
                m_log.Error("ERROR: An output file must be specified");
                m_log.Error(ToPNGInvocation());
                return;
            }

            m_log.Debug("ToPNG: infile={0}, outfile={1}", inFile, outFile);
            try {
                using (Stream inFileStream = new FileStream(inFile, FileMode.Open, FileAccess.Read)) {
                    using (Stream outFileStream = new FileStream(outFile, FileMode.Create, FileAccess.Write)) {
                        Converters.Converters.DoToPNG(inFileStream, outFileStream);
                    }
                }
            }
            catch (Exception e) {
                m_log.Error("ERROR: Could not use input/output files: " + e);
            }
        }

        private string ToPNGInvocation() {
            return @"ToPNG converts a JPGG2000 texture into a PNG formatted file
INVOCATION:
PrimConverters ToPNG
        -i|--input inputFilename
        -o|--output outputFilename
        -v|--verbose
";
        }
    }
}
