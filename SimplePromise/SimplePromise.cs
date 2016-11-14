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
    public class SimplePromise<T> {
        private enum ResolutionState {
            NoValueOrResolver,
            HaveResolver,
            HaveValue,
            ResolutionComplete
        };

        private ResolutionState resolverState;
        private Action<T> resolver;
        private ResolutionState rejectorState;
        private Action<Exception> rejecter;

        // We either get the value or the resolver first.
        private T resolveValue;
        private Exception rejectValue;

        public SimplePromise() {
            resolver = null;
            rejecter = null;
            resolverState = ResolutionState.NoValueOrResolver;
            rejectorState = ResolutionState.NoValueOrResolver;

        }

        public SimplePromise(Action<T> resolve, Action<Exception> reject) {
            resolver = resolve;
            rejecter = reject;
        }

        // Called by the one doing the action to complete the promise
        public void Resolve(T val) {
            switch (resolverState) {
                case ResolutionState.NoValueOrResolver:
                    resolveValue = val;
                    resolverState = ResolutionState.HaveValue;
                    break;
                case ResolutionState.HaveResolver:
                    resolver(val);
                    resolverState = ResolutionState.ResolutionComplete;
                    break;
                case ResolutionState.HaveValue:
                case ResolutionState.ResolutionComplete:
                    // NoOp since we should never get to this state
                    break;
            }
        }

        // Called by the one doing the action to reject the promise
        public void Reject(Exception e) {
            switch (rejectorState) {
                case ResolutionState.NoValueOrResolver:
                    rejectValue = e;
                    rejectorState = ResolutionState.HaveValue;
                    break;
                case ResolutionState.HaveResolver:
                    rejecter(e);
                    rejectorState = ResolutionState.ResolutionComplete;
                    break;
                case ResolutionState.HaveValue:
                case ResolutionState.ResolutionComplete:
                    // NoOp since we should never get to this state
                    break;
            }
        }

        public SimplePromise<T> Then(Action<T> resolve) {
            switch (resolverState) {
                case ResolutionState.NoValueOrResolver:
                    resolver = resolve;
                    resolverState = ResolutionState.HaveResolver;
                    break;
                case ResolutionState.HaveValue:
                    resolve(resolveValue);
                    resolverState = ResolutionState.ResolutionComplete;
                    break;
                case ResolutionState.HaveResolver:
                case ResolutionState.ResolutionComplete:
                    // NoOp since we should never get to this state
                    break;
            }
            return this;
        }

        public SimplePromise<T> Rejected(Action<Exception> reject) {
            switch (rejectorState) {
                case ResolutionState.NoValueOrResolver:
                    rejecter = reject;
                    rejectorState = ResolutionState.HaveResolver;
                    break;
                case ResolutionState.HaveValue:
                    reject(rejectValue);
                    rejectorState = ResolutionState.ResolutionComplete;
                    break;
                case ResolutionState.HaveResolver:
                case ResolutionState.ResolutionComplete:
                    // NoOp since we should never get to this state
                    break;
            }
            return this;
        }

    }
}
