// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using CoreWCF.IdentityModel.Policy;
using CoreWCF.IdentityModel.Selectors;
using CoreWCF.IdentityModel.Tokens;
using System.Security.Claims;
using System;
using System.Threading.Tasks;

namespace CoreWCF.Security
{
    /// <summary>
    /// Wraps a UserNameSecurityTokenHandler. Delegates the token authentication call to
    /// this wrapped tokenAuthenticator. Wraps the returned ClaimsIdentities into
    /// an IAuthorizationPolicy.
    /// </summary>
    internal class WrappedUserNameSecurityTokenAuthenticator : UserNameSecurityTokenAuthenticator
    {
        private readonly UserNameSecurityTokenHandler _wrappedUserNameSecurityTokenHandler;
        private readonly ExceptionMapper _exceptionMapper;

        /// <summary>
        /// Initializes an instance of <see cref="WrappedUserNameSecurityTokenAuthenticator"/>
        /// </summary>
        /// <param name="wrappedUserNameSecurityTokenHandler">The UserNameSecurityTokenHandler to wrap.</param>
        /// <param name="exceptionMapper">Converts token validation exceptions to SOAP faults.</param>
        public WrappedUserNameSecurityTokenAuthenticator(
            UserNameSecurityTokenHandler wrappedUserNameSecurityTokenHandler,
            ExceptionMapper exceptionMapper)
            : base()
        {
            _wrappedUserNameSecurityTokenHandler = wrappedUserNameSecurityTokenHandler ?? throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(wrappedUserNameSecurityTokenHandler));
            _exceptionMapper = exceptionMapper ?? throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(exceptionMapper));
        }

        /// <summary>
        /// Validates the token using the wrapped token handler and generates IAuthorizationPolicy
        /// wrapping the returned ClaimsIdentities.
        /// </summary>
        /// <param name="token">Token to be validated.</param>
        /// <returns>Read-only collection of IAuthorizationPolicy</returns>
        protected override ValueTask<ReadOnlyCollection<IAuthorizationPolicy>> ValidateTokenCoreAsync(SecurityToken token)
        {
            ReadOnlyCollection<ClaimsIdentity> identities = null;
            try
            {
                identities = _wrappedUserNameSecurityTokenHandler.ValidateToken(token);
            }
            catch (Exception ex)
            {
                if (!_exceptionMapper.HandleSecurityTokenProcessingException(ex))
                {
                    throw;
                }
            }

            List<IAuthorizationPolicy> policies = new List<IAuthorizationPolicy>(1);
            policies.Add(new AuthorizationPolicy(identities));
            return new ValueTask<ReadOnlyCollection<IAuthorizationPolicy>>(policies.AsReadOnly());
        }

        protected override ValueTask<ReadOnlyCollection<IAuthorizationPolicy>> ValidateUserNamePasswordCoreAsync(string userName, string password)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException(SR.Format(SR.ID4008, "WrappedUserNameSecurityTokenAuthenticator", "ValidateUserNamePasswordCore")));
        }
    }
}
