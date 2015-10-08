namespace TastyDomainDriven
{
    public class ReaderMatch<T>
    {
        public ReaderMatch(T item)
        {
            IsMatch = item != null;
            Result = item;
        }

        public T Result { get; set; }

        public bool IsMatch { get; set; }
    }
}