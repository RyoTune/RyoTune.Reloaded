using System.Diagnostics.CodeAnalysis;
using RyoTune.Reloaded.Common;

namespace RyoTune.Reloaded.Inis;

/// <summary>
/// INI service interface.
/// </summary>
public interface IIni : IRegisterMod
{
    /// <summary>
    /// Try to get a setting from an INI.
    /// </summary>
    /// <param name="iniId">ID of INI to get setting from.</param>
    /// <param name="name">Setting name.</param>
    /// <param name="value">Setting value, if found.</param>
    bool TryGetSetting(string iniId, string name, [NotNullWhen(true)] out string? value);

    /// <summary>
    /// Try to get a setting from an INI.
    /// </summary>
    /// <param name="iniId">ID of INI to get setting from.</param>
    /// <param name="name">Setting name.</param>
    /// <param name="section">Setting section, with <c>null</c> for global settings.</param>
    /// <param name="value">Setting value, if found.</param>
    bool TryGetSetting(string iniId, string name, string? section, [NotNullWhen(true)] out string? value);

    /// <summary>
    /// Try to get a setting from an INI.
    /// </summary>
    /// <param name="iniId">ID of INI to get setting from.</param>
    /// <param name="name">Setting name.</param>
    /// <param name="value">Setting value, if found.</param>
    bool TryGetSetting<TValue>(string iniId, string name, [NotNullWhen(true)] out TValue? value);

    /// <summary>
    /// Try to get a setting from an INI.
    /// </summary>
    /// <param name="iniId">ID of INI to get setting from.</param>
    /// <param name="name">Setting name.</param>
    /// <param name="section">Setting section, with <c>null</c> for global settings.</param>
    /// <param name="value">Setting value, if found.</param>
    bool TryGetSetting<TValue>(string iniId, string name, string? section, [NotNullWhen(true)] out TValue? value);

    /// <summary>
    /// Get a setting from an INI.
    /// </summary>
    /// <param name="iniId">INI ID.</param>
    /// <param name="name">Setting name.</param>
    /// <param name="section">Setting section, with <c>null</c> for global settings.</param>
    /// <returns>Setting value if found, <c>null</c> otherwise.</returns>
    string? GetSetting(string iniId, string name, string? section);

    /// <summary>
    /// Get the newest value of a setting.
    /// </summary>
    /// <param name="iniId">INI ID.</param>
    /// <param name="name">Setting name</param>
    /// <param name="section">Setting section, with <c>null</c> for global settings.</param>
    /// <param name="newValue">Callback given the latest value, starting with the current value.</param>
    void UsingSetting(string iniId, string name, string? section, Action<string> newValue);

    /// <summary>
    /// Get the newest value of a setting.
    /// </summary>
    /// <param name="iniId">INI ID.</param>
    /// <param name="name">Setting name</param>
    /// <param name="section">Setting section, with <c>null</c> for global settings.</param>
    /// <param name="newValue">Callback given the latest value, starting with the current value.</param>
    void UsingSetting<TValue>(string iniId, string name, string? section, Action<TValue> newValue);

    /// <summary>
    /// Get the newest setting values within a section.
    /// </summary>
    /// <param name="iniId">INI ID.</param>
    /// <param name="section">Setting section, with <c>null</c> for global settings.</param>
    /// <param name="newValues">Callback given dictionary with latest setting values, starting with the current values.</param>
    void UsingSettings(string iniId, string? section, Action<IReadOnlyDictionary<string, string>> newValues);
}