namespace Browser.Selector.Lib.Interfaces
{
    public interface IBrowser
    {
        public string Name { get; }
        public string ExecutablePath { get; }

        public void Open(string Url, bool runAs = false);
    }
}
