// --------------------------------------------------------------------------
// <copyright file="TestRunInfo.cs" company="Devart">
//
// Copyright (c) Devart. ALL RIGHTS RESERVED
// The entire contents of this file is protected by International Copyright Laws.
// Unauthorized reproduction, and distribution of all or any portion of the code
// contained in this file is strictly prohibited and may result in severe civil
// and criminal penalties and will be prosecuted to the maximum extent possible.
//
// RESTRICTIONS
//
// THIS SOURCE CODE AND ALL RESULTING INTERMEDIATE FILES ARE CONFIDENTIAL AND
// PROPRIETARY TRADE SECRETS OF DEVART.
//
// THE SOURCE CODE CONTAINED WITHIN THIS FILE AND ALL RELATED FILES OR ANY PORTION
// OF ITS CONTENTS SHALL AT NO TIME BE COPIED, TRANSFERRED, SOLD, DISTRIBUTED,
// OR OTHERWISE MADE AVAILABLE TO OTHER INDIVIDUALS WITHOUT EXPRESS WRITTEN CONSENT
// AND PERMISSION FROM DEVART.
// </copyright>
// --------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace ProducerConsumerConsoleApp
{
  public sealed class TestRunInfo
  {
    private List<Exception> exceptions;

    public TestRunInfo()
    {
    }

    public Guid Id
    {
      get;
      internal set;
    }

    public Guid EnvironmentId
    {
      get;
      internal set;
    }

    public string Name
    {
      get;
      internal set;
    }

    public string Directory
    {
      get;
      internal set;
    }

    public TimeSpan Duration
    {
      get;
      internal set;
    }

    public TestStatus Status
    {
      get;
      internal set;
    }

    public Exception Exception
    {
      get;
      internal set;
    }

   // public IConditionalAssertion Assertion { get; set; }

    public IEnumerable<Exception> PostrunExceptions => this.exceptions == null ? Enumerable.Empty<Exception>() : this.exceptions.AsReadOnly();

    internal void AppendPostrunException(Exception exception)
    {
      if (this.exceptions == null)
      {
        this.exceptions = new List<Exception>();
      }

      //Status |= TestStatus.Warning;
      this.exceptions.Add(exception);
    }
  }
}