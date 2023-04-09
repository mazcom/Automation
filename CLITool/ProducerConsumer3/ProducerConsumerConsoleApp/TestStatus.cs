// --------------------------------------------------------------------------
// <copyright file="TestStatus.cs" company="Devart">
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

namespace ProducerConsumerConsoleApp
{
  [Flags]
  public enum TestStatus
  {
    None = 0,
    Stopped = 1,
    Warning = 2,
    Success = 4,
    NoAssertion = 8,
    NoCategory = 16,
    NotFound = 32,
    NotExecuted = 64,
    Failed = 128,
    Bad = 256,
    Running = 512,
    Started = 1024,
    Skipped = 2048
  }
}
