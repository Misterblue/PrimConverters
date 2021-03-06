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
// Derived from DSGLogToolkit, BSD 3-clause license.
//      Copyright (c) 2013 Intel Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;

namespace org.herbal3d.tools.ParameterParsing
{
public static class ParameterParse
{
    public const string FIRST_PARAM = "--firstParameter";
    public const string LAST_PARAM = "--lastParameter";
    public const string ERROR_PARAM = "--errorParameter";
    
    // ================================================================
    /// <summary>
    /// Given the array of command line arguments, create a dictionary of the parameter
    /// keyword to values. If there is no value for a parameter keyword, the value of
    /// 'null' is stored.
    /// Command line keywords begin with "-" or "--". Anything else is presumed to be
    /// a value.
    /// </summary>
    /// <param name="args">array of command line tokens</param>
    /// <param name="firstOpFlag">if 'true' presume the first token in the parameter line
    /// is a special value that should be assigned to the keyword "--firstparam".</param>
    /// <param name="multipleFiles">if 'true' presume multiple specs at the end of the line
    /// are filenames and pack them together into a CSV string in LAST_PARAM.</param>
    /// <returns></returns>
    public static Dictionary<string, string> ParseArguments(string[] args, bool firstOpFlag, bool multipleFiles) {
        Dictionary<string, string> m_params = new Dictionary<string, string>();

        for (int ii = 0; ii < args.Length; ii++) {
            string para = args[ii];
            // is this a parameter?
            if (para[0] == '-') {
                // is the next one a parameter?
                if (ii == (args.Length - 1) || args[ii + 1][0] == '-') {
                    // two parameters in a row. this must be a toggle parameter
                    m_params.Add(para, null);
                }
                else {
                    // looks like a parameter followed by a value
                    m_params.Add(para, args[ii + 1]);
                    ii++;       // skip the value we just added to the dictionary
                }
            }
            else {
                if (ii == 0 && firstOpFlag) {
                    // if the first thing is not a parameter, make like it's an op or something
                    m_params.Add(FIRST_PARAM, para);
                }
                else {
                    if (multipleFiles) {
                        // Pack all remaining arguments into a comma-separated list as LAST_PARAM
                        StringBuilder multFiles = new StringBuilder();
                        for (int jj = ii; jj < args.Length; jj++) {
                            if (multFiles.Length != 0) {
                                multFiles.Append(",");
                            }
                            multFiles.Append(args[jj]);
                        }
                        m_params.Add(LAST_PARAM, multFiles.ToString());

                        // Skip them all
                        ii = args.Length;
                    }
                    else {
                        // This token is not a keyword. If it's the last thing, place it
                        // into the dictionary as the last parameter. Otherwise an error.
                        if (ii == args.Length - 1) {
                            m_params.Add(LAST_PARAM, para);
                        }
                        else {
                            // something is wrong with  the format of the parameters
                            m_params.Add(ERROR_PARAM, "Unknown parameter " + para);
                        }
                    }
                }
            }
        }
        return m_params;
    }
}
}
