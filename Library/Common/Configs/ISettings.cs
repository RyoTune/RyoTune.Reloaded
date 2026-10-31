using System.Diagnostics.CodeAnalysis;

namespace RyoTune.Reloaded.Common.Configs;

/// <summary>
/// General settings service.
/// </summary>
public interface ISettings
{
    /// <summary>
    /// Try to get a setting from a config.
    /// </summary>
    /// <param name="configId">ID of config to get setting from.</param>
    /// <param name="name">Setting name.</param>
    /// <param name="value">Setting value, if found.</param>
    bool TryGetSetting(string configId, string name, [NotNullWhen(true)] out string? value);

    /// <summary>
    /// Try to get a setting from a config.
    /// </summary>
    /// <param name="configId">ID of config to get setting from.</param>
    /// <param name="name">Setting name.</param>
    /// <param name="section">Setting section, with <c>null</c> for global settings.</param>
    /// <param name="value">Setting value, if found.</param>
    bool TryGetSetting(string configId, string name, string? section, [NotNullWhen(true)] out string? value);

    /// <summary>
    /// Try to get a setting from a config.
    /// </summary>
    /// <param name="configId">ID of config to get setting from.</param>
    /// <param name="name">Setting name.</param>
    /// <param name="value">Setting value, if found.</param>
    bool TryGetSetting<TValue>(string configId, string name, [NotNullWhen(true)] out TValue? value);

    /// <summary>
    /// Try to get a setting from a config.
    /// </summary>
    /// <param name="configId">ID of config to get setting from.</param>
    /// <param name="name">Setting name.</param>
    /// <param name="section">Setting section, with <c>null</c> for global settings.</param>
    /// <param name="value">Setting value, if found.</param>
    bool TryGetSetting<TValue>(string configId, string name, string? section, [NotNullWhen(true)] out TValue? value);

    /// <summary>
    /// Get a setting from a config.
    /// </summary>
    /// <param name="configId">Config ID.</param>
    /// <param name="name">Setting name.</param>
    /// <param name="section">Setting section, with <c>null</c> for global settings.</param>
    /// <returns>Setting value if found, <c>null</c> otherwise.</returns>
    string? GetSetting(string configId, string name, string? section);

    /// <summary>
    /// Get the newest value of a setting.
    /// </summary>
    /// <param name="configId">Config ID.</param>
    /// <param name="name">Setting name</param>
    /// <param name="section">Setting section, with <c>null</c> for global settings.</param>
    /// <param name="newValue">Callback given the latest value, starting with the current value.</param>
    void UsingSetting(string configId, string name, string? section, Action<string> newValue);

    /// <summary>
    /// Get the newest value of a setting.
    /// </summary>
    /// <param name="configId">Config ID.</param>
    /// <param name="name">Setting name</param>
    /// <param name="section">Setting section, with <c>null</c> for global settings.</param>
    /// <param name="newValue">Callback given the latest value, starting with the current value.</param>
    void UsingSetting<TValue>(string configId, string name, string? section, Action<TValue> newValue);

    /// <summary>
    /// Get the newest setting values within a section.
    /// </summary>
    /// <param name="configId">Config ID.</param>
    /// <param name="section">Setting section, with <c>null</c> for global settings.</param>
    /// <param name="newValues">Callback given dictionary with latest setting values, starting with the current values.</param>
    void UsingSettings(string configId, string? section, Action<IReadOnlyDictionary<string, string>> newValues);
}