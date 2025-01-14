// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Xml.Linq;

namespace Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;

/// <summary>
/// A class that can deserialize an <see cref="XElement"/> that represents the serialized version
/// of an <see cref="ManagedAuthenticatedEncryptorDescriptor"/>.
/// </summary>
public sealed class ManagedAuthenticatedEncryptorDescriptorDeserializer : IAuthenticatedEncryptorDescriptorDeserializer
{
    /// <summary>
    /// Imports the <see cref="ManagedAuthenticatedEncryptorDescriptor"/> from serialized XML.
    /// </summary>
    [RequiresUnreferencedCode(TrimmerWarning.Message)]
    public IAuthenticatedEncryptorDescriptor ImportFromXml(XElement element)
    {
        if (element == null)
        {
            throw new ArgumentNullException(nameof(element));
        }

        // <descriptor>
        //   <!-- managed implementations -->
        //   <encryption algorithm="..." keyLength="..." />
        //   <validation algorithm="..." />
        //   <masterKey>...</masterKey>
        // </descriptor>

        var configuration = new ManagedAuthenticatedEncryptorConfiguration();

        var encryptionElement = element.Element("encryption")!;
        configuration.EncryptionAlgorithmType = FriendlyNameToType((string)encryptionElement.Attribute("algorithm")!);
        configuration.EncryptionAlgorithmKeySize = (int)encryptionElement.Attribute("keyLength")!;

        var validationElement = element.Element("validation")!;
        configuration.ValidationAlgorithmType = FriendlyNameToType((string)validationElement.Attribute("algorithm")!);

        Secret masterKey = ((string)element.Element("masterKey")!).ToSecret();

        return new ManagedAuthenticatedEncryptorDescriptor(configuration, masterKey);
    }

    // Any changes to this method should also be be reflected
    // in ManagedAuthenticatedEncryptorDescriptor.TypeToFriendlyName.
    [RequiresUnreferencedCode(TrimmerWarning.Message)]
    private static Type FriendlyNameToType(string typeName)
    {
        if (typeName == nameof(Aes))
        {
            return typeof(Aes);
        }
        else if (typeName == nameof(HMACSHA1))
        {
            return typeof(HMACSHA1);
        }
        else if (typeName == nameof(HMACSHA256))
        {
            return typeof(HMACSHA256);
        }
        else if (typeName == nameof(HMACSHA384))
        {
            return typeof(HMACSHA384);
        }
        else if (typeName == nameof(HMACSHA512))
        {
            return typeof(HMACSHA512);
        }
        else
        {
            return Type.GetType(typeName, throwOnError: true)!;
        }
    }
}
