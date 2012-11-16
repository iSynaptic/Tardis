// The MIT License
// 
// Copyright (c) 2012 Jordan E. Terrell
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using iSynaptic.Commons;
using iSynaptic.Commons.Linq;

namespace Tardis
{
    public class Universe : Node
    {
        private readonly Subject<ChangeSet> _CommittedChangeSets 
            = new Subject<ChangeSet>();

        public class Created : Change
        {
            public Created(Guid nodeId) : base(nodeId)
            {
            }
        }

        public static Universe Create()
        {
            var change = new Created(Guid.NewGuid());
            return new Universe(change);
        }

        private Universe(Created change) : base(change)
        {
            Id = change.NodeId;
        }

        public void Change(int basedOnVersion, Action<Universe> changes)
        {
            Guard.NotNull(changes, "changes");
            MakeChanges(basedOnVersion, changes);
        }

        public IObservable<ChangeSet> ObserveCommittedChangeSets(bool observeHistoricalOnSubscribe)
        {
            if (!observeHistoricalOnSubscribe)
                return _CommittedChangeSets;

            return Observable.Create<ChangeSet>(obs =>
            {
                var lastChangeSet = GetLastChangeSet();
                foreach (var changeSet in lastChangeSet.Recurse(x => x.Previous).Reverse())
                {
                    obs.OnNext(changeSet);
                }

                return _CommittedChangeSets.Subscribe(obs);
            });
        }

        public ChangeSet LastChangeSet { get { return GetLastChangeSet(); } }
    }
}
