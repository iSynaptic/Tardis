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
using System.Collections.Generic;
using iSynaptic.Commons;

namespace Tardis
{
    public abstract class Node
    {
        private ChangeSet _LastChangeSet;
        private List<Change> _PendingChanges;

        public class Annotated : Change
        {
            public Annotated(Guid nodeId, Annotation annotation) : base(nodeId)
            {
                Annotation = Guard.NotNull(annotation, "annotation");
            }

            public Annotation Annotation { get; private set; }
        }

        internal Node(Change initialChange)
        {
            Guard.NotNull(initialChange, "initialChange");
            
            Root = this;
            _LastChangeSet = new ChangeSet(new []{initialChange});
        }

        protected Node(Node parent)
        {
            Parent = Guard.NotNull(parent, "parent").ToMaybe();
            Root = parent.Root;
        }

        protected bool CanChange()
        {
            if (Parent.HasValue)
                return Root.CanChange();

            return _PendingChanges != null;
        }

        public void Annotate(Annotation annotation)
        {
            RecordChange(new Annotated(Id, annotation));
        }

        protected void RecordChange(Change change)
        {
            if (Parent.HasValue)
            {
                Root.RecordChange(change);
                return;
            }

            if (!CanChange())
                throw new InvalidOperationException("You cannot record changes without calling BeginChanges.");

            _PendingChanges.Add(change);
        }

        protected void MakeChanges<T>(int basedOnVersion, Action<T> changes) 
            where T : Node
        {
            if(Parent.HasValue)
                throw new InvalidOperationException("BeginChanges can only be called by the root node.");

            T root = this as T;
            if (root == null)
                throw new ArgumentException("The changes delegate must accept the root type.", "changes");

            if (root._LastChangeSet.Version != basedOnVersion)
                throw new InvalidOperationException("The based on version does not match the current version.");

            try
            {
                _PendingChanges = new List<Change>();
                changes(root);
                
                _LastChangeSet = new ChangeSet(_LastChangeSet, _PendingChanges);
                _PendingChanges = null;

                OnCommittedChangeSet(_LastChangeSet);
            }
            finally
            {
                _PendingChanges = null;
            }
        }

        protected virtual void OnCommittedChangeSet(ChangeSet changeSet) { }

        protected ChangeSet GetLastChangeSet()
        {
            return Root._LastChangeSet;
        }

        public Guid Id { get; protected set; }
        public int Version { get { return GetLastChangeSet().Version; } }

        public Maybe<Node> Parent { get; private set; }
        public Node Root { get; private set; }
    }
}