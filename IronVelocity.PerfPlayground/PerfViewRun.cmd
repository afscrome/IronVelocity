pushd %~d0
cd bin\debug
start perfview /Merge:False /Providers:"*IronVelocity-TemplateGeneration::Verbose,*IronVelocity-Binding::Verbose" run IronVelocity.PerfPlayground.exe
popd