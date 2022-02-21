$scripts = 'test1.ps1', 'test2.ps1', 'test3.ps1', 'test4.ps1', 'test5.ps1'

$scripts | ForEach-Object {
  Start-Job -FilePath $_
} | Wait-Job | Receive-Job

Start-Job { C:\JUNK\Code\PowerScript\CS1.ps1 $a }
Start-Job { C:\JUNK\Code\PowerScript\CS2.ps1 $b }