namespace UnityBestPractices.Editor.Validator
{
    public interface IValidator
    {
        string Name { get; }
        string Description { get; }
        ValidationResult Validate();
    }
}
