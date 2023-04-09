// --------------------------------------------------------------------------
// <copyright file="EnvironmentRunInfo.cs" company="Devart">
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
using System.Runtime.CompilerServices;


namespace ProducerConsumerConsoleApp
{
  public sealed class EnvironmentRunInfo
  {
    private List<Exception> exceptions;

    public Guid Id
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

    public EnvironmentStatus Status
    {
      get;
      internal set;
    }

    public TimeSpan BuildDuration
    {
      get;
      internal set;
    }

    public TimeSpan CleanupDuration
    {
      get;
      internal set;
    }

    public Exception BuildException
    {
      get;
      internal set;
    }

    public static EnvironmentRunInfo Empty() => new EnvironmentRunInfo()
    {
      Id = Guid.Empty,
      Name = "No Environment",
      Status = EnvironmentStatus.BuildSuccess
    };

    public IEnumerable<Exception> CleanupExceptions => this.exceptions == null ? Enumerable.Empty<Exception>() : this.exceptions.AsReadOnly();

    internal void AppendCleanupException(Exception exception)
    {
      if (this.exceptions == null)
      {
        this.exceptions = new List<Exception>();
      }

      this.exceptions.Add(exception);
      //Status |= EnvironmentStatus.CleanupFailure;
    }
  }
}
