namespace Features.Input
{
    public struct Tracked<T> where T : struct
    {
        public T data;
        public bool isChanged;

        public bool PopIsChanged 
        {
            get
            {
                bool value = isChanged;
                isChanged = false;
                return value;
            }
        }
    }
}