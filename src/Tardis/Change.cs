using System;
using iSynaptic.Commons;

namespace Tardis
{
    public abstract class Change
    {
        protected Change(Guid nodeId)
        {
            NodeId = Guard.NotEmpty(nodeId, "nodeId");
        }

        public Guid NodeId { get; private set; }
    }
}