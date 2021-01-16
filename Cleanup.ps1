$dirs = $data = @('dll\x86','x86','cs','de','es','fr','it','ja','ko','pl','pt-BR','ru','tr','zh-Hans','zh-Hant')
foreach($d in $dirs)
{
	Remove-Item .\App.Wpf\bin\Release\win10-x64\publish\$d -Recurse -ErrorAction SilentlyContinue
}
