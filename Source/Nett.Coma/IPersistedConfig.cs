namespace Nett.Coma
{
    public interface IPersistedConfig<T>
    {
        bool EnsureExists(T def);

        T Load();

        void Save(T cfg);
    }
}
