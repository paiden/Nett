namespace Nett
{
    public interface ITomlObjectVisitor
    {
        void Visit(TomlTable table);

        void Visit(TomlTableArray tableArray);

        void Visit(TomlInt i);

        void Visit(TomlFloat f);

        void Visit(TomlBool b);

        void Visit(TomlString s);

        void Visit(TomlTimeSpan ts);

        void Visit(TomlDateTime dt);

        void Visit(TomlArray a);
    }
}
