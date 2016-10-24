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

namespace org.herbal3d.tools.Logging {

    public class Logger : IDisposable {

        private static Logger m_instance = null;
        public static Logger Instance() {
            if (m_instance == null) {
                m_instance = new Logger();
            }
            return m_instance;
        }

        public LOGLEVEL LogLevel { get; set; }

        public enum LOGLEVEL {
            NONE, ERROR, INFO, DEBUG
        };

        public Logger() {
        }
    
        public void Info(string msg, params Object[] args) {
            if (LogLevel == LOGLEVEL.INFO || LogLevel == LOGLEVEL.DEBUG) {
                OutTheLine(msg, args);
            }
        }

        public void Debug(string msg, params Object[] args) {
            if (LogLevel == LOGLEVEL.DEBUG) {
                OutTheLine(msg, args);
            }
        }

        public void Error(string msg, params Object[] args) {
            OutTheLine(msg, args);
        }

        private void OutTheLine(string msg, params Object[] args) {
            System.Console.WriteLine(msg, args);
        }

        // For the day this does more than just write to the console.
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
        // ~Logger() {
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
