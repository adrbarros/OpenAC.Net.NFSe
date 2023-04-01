﻿// ***********************************************************************
// Assembly         : OpenAC.Net.NFSe
// Author           : Diego Martins
// Created          : 08-30-2021
//
// Last Modified By : Rafael Dias
// Last Modified On : 27-08-2022
// ***********************************************************************
// <copyright file="NFSeRestServiceClient.cs" company="OpenAC .Net">
//		        		   The MIT License (MIT)
//	     		    Copyright (c) 2014 - 2022 Projeto OpenAC .Net
//
//	 Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//	 The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//	 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using OpenAC.Net.Core.Extensions;

namespace OpenAC.Net.NFSe.Providers;

public abstract class NFSeRestServiceClient : NFSeHttpServiceClient
{
    #region Constructors

    /// <summary>
    ///
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="tipoUrl"></param>
    protected NFSeRestServiceClient(ProviderBase provider, TipoUrl tipoUrl) : base(provider, tipoUrl, provider.Certificado)
    {
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="tipoUrl"></param>
    /// <param name="certificado"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    protected NFSeRestServiceClient(ProviderBase provider, TipoUrl tipoUrl, X509Certificate2 certificado) : base(provider, tipoUrl, certificado)
    {
    }

    #endregion Constructors

    #region Properties

    public string AuthenticationHeader { get; protected set; } = "Authorization";

    #endregion Properties

    #region Methods

    protected string Get(string action, string contentyType)
    {
        var url = Url;

        try
        {
            SetAction(action);
            EnvelopeEnvio = string.Empty;

            var auth = Authentication();
            var headers = !auth.IsEmpty() ? new Dictionary<string, string> { { AuthenticationHeader, auth } } : null;

            Execute(contentyType, HttpMethod.Get, headers);
            return EnvelopeRetorno;
        }
        finally
        {
            Url = url;
        }
    }

    protected string Post(string action, string message, string contentyType)
    {
        var url = Url;

        try
        {
            SetAction(action);

            var auth = Authentication();
            var headers = !auth.IsEmpty() ? new Dictionary<string, string> { { AuthenticationHeader, auth } } : null;

            EnvelopeEnvio = message;

            Execute(contentyType, HttpMethod.Post, headers);
            return EnvelopeRetorno;
        }
        finally
        {
            Url = url;
        }
    }

    protected string Upload(string action, string message, bool executeSetAction = true)
    {
        var url = Url;

        try
        {
            if (executeSetAction)
                SetAction(action);

            var auth = Authentication();
            var headers = !auth.IsEmpty() ? new Dictionary<string, string> { { AuthenticationHeader, auth } } : null;

            EnvelopeEnvio = message;

            var fileName = $"{DateTime.Now:yyyyMMddssfff}_{PrefixoEnvio}_envio.xml";
            GravarSoap(EnvelopeEnvio, fileName);

            Execute("", HttpMethod.Post, headers);

            GravarSoap(EnvelopeRetorno, $"{DateTime.Now:yyyyMMddssfff}_{PrefixoResposta}_retorno.xml");

            return EnvelopeRetorno;
        }
        finally
        {
            Url = url;
        }
    }

    protected virtual string Authentication() => "";

    protected void SetAction(string action)
    {
        Url ??= "";
        Url = !Url.EndsWith("/") ? $"{Url}/{action}" : $"{Url}{action}";
    }

    #endregion Methods
}