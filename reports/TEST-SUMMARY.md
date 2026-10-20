# Test summary

Sanitized Windows x64 verification of the staged AutoEtabs build.

- Date: 2026-10-20
- Environment: Windows 11 x64 virtual machine
- Tested file: `Auto ETABS 5.exe` (staged build)
- SHA256: `a69c6b3b2ee0a95ff80a549dfcb888f099a0488391f4d90024b47f859b953058`
- Result: application launched and stayed responsive through the observation period
- Interaction: the Stories command was invoked successfully; the Stories dialog opened
  (story list, OK/Cancel controls present; 689 UI elements after invocation)
- Raw machine logs and screenshots are kept locally and are not published.

Full ETABS/SAFE integration was not exercised because the CSI applications
were not installed in the test virtual machine.
