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

namespace org.herbal3d.tools.SimplePromise
{
    /* A simple implementation of the Promise interface.
     * .NET does many things in a complicated and non-standard way.
     * This class creates a simple wrapper for Task<T> and TaskCompletionSource<T>
     *    that looks more like the Promise/Future interface used in other languages.
     *    There exist complete and featureful alternatives
     *    (https://github.com/Real-Serious-Games/C-Sharp-Promise for instance)
     *    but this is a very simple pattern that is not dependent on any external
     *    library or package.
     *
     *  Only implements the simple:
     *    SimplePromise<T> someDay = new SimplePromise(resolver, rejecter);
     *      or
     *    SimplePromise<T> someDay = new SimplePromise();
     *    somday.Then(resolver).Rejected(rejecter);
     *  The execution routine calls:
     *    someDay.Resolve(T value);
     *       or
     *    someDay.Reject(Exception e);
     */
    public class SimplePromise<T>
    {
        private Action<T> resolver = null;
        private Action<Exception> rejecter = null;

        public SimplePromise() {
            resolver = null;
            rejecter = null;
        }

        public SimplePromise(Action<T> resolve, Action<Exception> reject) {
            resolver = resolve;
            rejecter = reject;
        }

        // Called by the one doing the action to complete the promise
        public void Resolve(T val) {
            if (resolver != null) {
                resolver(val);
            }
        }

        // Called by the one doing the action to reject the promise
        public void Reject(Exception e) {
            if (rejecter != null) {
                rejecter(e);
            }
        }

        public void Then(Action<T> resolve) {
            resolver = resolve;
        }

        public void Rejected(Action<Exception> reject) {
            rejecter = reject;
        }

    }
}
