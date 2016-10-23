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
using ParameterParsing;
using Logging;

namespace org.herbal3d.PrimConverters {
    class PrimConverters {

        Dictionary<string, string> m_Parameters;
        int m_ParamVerbose = 0;

        string m_inFile;
        string m_outFile;
        string m_op;

        private bool IsVerbose { get { return m_ParamVerbose > 0; } }
        private bool IsVeryVerbose { get { return m_ParamVerbose > 1; } }

        private string Invocation() {
            return @"Invocation:
INVOCATION:
PrimConverters op
        -i|--input inputFilename
        -o|--output outputFilename
        --verbose

        where 'op' must be one of:
";
        }

        static void Main(string[] args) {
            PrimConverters prog = new PrimConverters();
            prog.Start(args);
            return;
        }

        public PrimConverters() {
        }

        public void Start(string[] args) {
            m_Parameters = ParameterParse.ParseArguments(args, false /* firstOpFlag */, true /* multipleFiles */);
            foreach (KeyValuePair<string, string> kvp in m_Parameters) {
                switch (kvp.Key) {
                    case ParameterParse.FIRST_PARAM:
                        m_op = kvp.Value.ToLower();
                        break;
                    case "-i":
                    case "--input":
                        m_inFile = kvp.Value;
                        break;
                    case "-o":
                    case "--output":
                        m_outFile = kvp.Value;
                        break;
                    case "--verbose":
                        m_ParamVerbose++;
                        break;
                    case ParameterParse.ERROR_PARAM:
                        // if we get here, the parser found an error
                        Logger.Log("Parameter error: " + kvp.Value);
                        Logger.Log(Invocation());
                        return;
                    default:
                        Logger.Log("ERROR: UNKNOWN PARAMETER: " + kvp.Key);
                        Logger.Log(Invocation());
                        return;
                }
            }

            // Verify parameters
            if (String.IsNullOrEmpty(m_op)) {
                Logger.Log("ERROR: An operation must be specified");
                Logger.Log(Invocation());
                return;
            }

            // Do the requested conversion

        }
    }
}
