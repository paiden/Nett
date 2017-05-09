namespace Nett.Coma.Path
{
    internal sealed partial class TPath
    {
        public interface ITPathSegment
        {
            TomlObject Apply(TomlObject obj);

            TomlObject ApplyOrCreate(TomlObject obj);

            TomlObject TryApply(TomlObject obj);

            void SetValue(TomlObject value);
        }
    }
}
