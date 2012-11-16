namespace Tardis
{
    public abstract class Annotation
    {
        public virtual bool CanAnnotate(Node annotatable) { return true; }
    }
}