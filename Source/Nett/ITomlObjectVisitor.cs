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

        void Visit(TomlDuration ts);

        void Visit(TomlOffsetDateTime dt);

        void Visit(TomlLocalDate ld);

        void Visit(TomlLocalDateTime ldt);

        void Visit(TomlLocalTime lt);

        void Visit(TomlArray a);
    }
}
