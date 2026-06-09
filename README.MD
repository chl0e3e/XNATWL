# XNATWL

A port of TWL to the C# XNA/FNA engine. The original TWL is written in Java and supports LWJGL 2.
Multiple components are of open-source origin, including TWL itself.

### Features

- XNA rendering (under XNATWL.Renderer.XNA)
- Multi-line font caching
- Same HTML/CSS parser as the Java version
- Slightly different model system (where HasCallback features used in the Java version have been replaced by the event model offered by C#)
- Same XML and template engine so you can bring your themes from TWL
- Demos matching the TWL examples

## Licenses

### TWL

This projects ports a large amount of code found in the Java project TWL which is the intellectual property of Matthias Mann.

- http://hg.l33tlabs.org
- https://github.com/MatthiasMann/twl

```
Copyright (c) 2008-2012, Matthias Mann

All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

    * Redistributions of source code must retain the above copyright notice,
      this list of conditions and the following disclaimer.
    * Redistributions in binary form must reproduce the above copyright
      notice, this list of conditions and the following disclaimer in the
      documentation and/or other materials provided with the distribution.
    * Neither the name of Matthias Mann nor the names of its contributors may
      be used to endorse or promote products derived from this software
      without specific prior written permission.
    * Redistributions in source or binary form must keep the original package
      and class name.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
"AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
```

### BigRational

This project uses the BigRational class in place of the Number utility found in Java. This allows XNATWL to operate with big and small numbers disregarding their fractional natures or the nature of the upper bound maximums of value types.
```
Microsoft Public License (MS-PL)
This license governs use of the accompanying software. If you use the software, you accept this license. If you do not accept the license, do not use the software.

1. Definitions
The terms "reproduce," "reproduction," "derivative works," and "distribution" have the
same meaning here as under U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the software.

A "contributor" is any person that distributes its contribution under this license.

"Licensed patents" are a contributor's patent claims that read directly on its contribution.

2. Grant of Rights

(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.

(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

3. Conditions and Limitations

(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.

(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.

(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.

(D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.

(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement.
```

### XMLPULL
The `XNATWL.Utils.XMLParser` class ported from the Java version `de.matthiasmann.twl.utils.XMLParser` implements and expands various functions of the XMLPull API normally found in a jar such as `xpp3-1.1.4c.jar`. It uses `System.Xml` in a certain way instead of a fully conforming port of the Java library.

```
XMLPULL API IS FREE
-------------------

All of the XMLPULL API source code, compiled code, and documentation 
contained in this distribution *except* for tests (see separate LICENSE_TESTS.txt)
are in the Public Domain.

XMLPULL API comes with NO WARRANTY or guarantee of fitness for any purpose.

Initial authors:

  Stefan Haustein
  Aleksander Slominski

2001-12-12
```

## Removed third-party code (de-GPL / de-copyleft)

The classes below previously carried GPL or other restrictive licenses. They have been removed or
re-implemented so the project no longer depends on those licenses. These changes were made from the
public API contract and standard, non-creative algorithms — not by copying the original sources —
but note this is not a formal "clean-room" process and is not legal advice. Have counsel review if a
strict provenance guarantee is required.

- **`XNATWL.Util.BitSet`** — previously a port of `java.util.BitSet` taken from GNU Classpath
  (**GPLv2 + Classpath exception**). Re-implemented from scratch against the public `java.util.BitSet`
  API contract using standard word-array storage and well-known public bit algorithms (SWAR
  population count, binary-search trailing-zero count). No GPL / Classpath source was consulted while
  writing the replacement, and it carries no GPL obligation.

- **`XNATWL.Utils.BitOperations.RightMove`** — previously copied from a Stack Overflow answer
  (https://stackoverflow.com/a/41907678) under **CC-BY-SA 3.0**. The method only emulates Java's
  unsigned right-shift (`>>>`); it has been replaced with the native C# idiom
  `(int)((uint)value >> pos)`, which is a basic language operation and not the original author's
  copyrighted expression.

- **`XNATWL.Utils.ObjectCopier`** — previously licensed under the **Code Project Open License
  (CPOL) 1.02**. It performed a `BinaryFormatter` deep clone (removed in .NET 8) and had no callers,
  so it has been deleted entirely.
