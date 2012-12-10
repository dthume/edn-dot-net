﻿namespace EDNReaderTest
module test =
    open FParsec
    open EDNReaderWriter
    open EDNReaderWriter.EDNReader
    open EDNReaderWriter.EDNWriter
    open EDNReaderTestCS
    open System.Diagnostics
    open System.IO


    let time (f : (unit -> 'a)) = 
        let stopWatch = Stopwatch.StartNew()
        let result = f ()
        stopWatch.Stop()
        printfn "%f" stopWatch.Elapsed.TotalMilliseconds
        result

    let TEST_STR_SmallHierarchy =
        @"[;comment 1
                :pre/asdf
                    [1 2 3 ""Asdfsaf"" 5 6 7 #{""a"" 1 2 [7 8 9], nil} {[1 2] ,#{78 12} nil 1
                    {#_ 1 nil :value} 3 ;comment 2

        ,, ""asdfa"" 4 :foo nil} foo #inst ""1999-03-11T23:15:36.11Z""]]"

    let TEST_STR_CustomTypeTimezone =
        "#sample-custom-type/timezone {:Id \"Custom Time Zone\" :BaseUtcOffset \"02:00:00\" :DisplayName \"(UTC+02:00) Custom Time Zone\" :StandardDisplayName \"Custom Time Zone\"}"

    let testReadString = EDNReaderFuncs.readString(TEST_STR_SmallHierarchy)

    let testCustomHandler =
        let r1 = EDNReaderFuncs.readString(TEST_STR_CustomTypeTimezone, new SampleCustomHandler()).Head
        EDNWriterFuncs.writeString(r1, new SampleCustomPrinter())

    let testEquality =
        let mutable r1 = EDNReaderFuncs.readString(TEST_STR_SmallHierarchy).Head
        let mutable r2 = EDNReaderFuncs.readString(TEST_STR_SmallHierarchy).Head
        Funcs.Assert(r1.Equals(r2))

        r1 <- EDNReaderFuncs.readString("[[1 nil 1]]").Head
        r2 <- EDNReaderFuncs.readString("[[nil 1 nil]]").Head
        Funcs.Assert(not(r1.Equals(r2)))

        r1 <- EDNReaderFuncs.readString("[[nil 1 nil]]").Head
        r2 <- EDNReaderFuncs.readString("[[1 nil]]").Head
        Funcs.Assert(not(r1.Equals(r2)))

        r1 <- EDNReaderFuncs.readString("[{nil 1 1 nil}]").Head
        r2 <- EDNReaderFuncs.readString("[{nil 1 1 2}]").Head
        Funcs.Assert(not(r1.Equals(r2)))

        r1 <- EDNReaderFuncs.readString("[(nil 1 1 2)]").Head
        r2 <- EDNReaderFuncs.readString("[(4 1 1 nil)]").Head
        Funcs.Assert(not(r1.Equals(r2)))

        r1 <- EDNReaderFuncs.readString("[#{nil 1 2 3}]").Head
        r2 <- EDNReaderFuncs.readString("[#{nil 1 2 3}]").Head
        Funcs.Assert(r1.Equals(r2));

        r1 <- EDNReaderFuncs.readString("[#{nil 1 2 3}]").Head
        r2 <- EDNReaderFuncs.readString("[#{5 nil 1 6}]").Head
        Funcs.Assert(not(r1.Equals(r2)))

        r1 <- EDNReaderFuncs.readFile("..\\..\\..\\TestData\\hierarchical.edn");
        r2 <- EDNReaderFuncs.readFile("..\\..\\..\\TestData\\hierarchical.edn");
        Funcs.Assert(r1.Equals(r2));

    let testReadFile = EDNReaderFuncs.readFile("..\\..\\..\\TestData\\hierarchical.edn")

    let testWriter =
        let r1 = EDNReaderFuncs.readString(TEST_STR_SmallHierarchy)
        for ednObj in r1 do
            let teststr = EDNWriterFuncs.writeString(ednObj)
            printf "Print String: %s" teststr

            use ms = new MemoryStream()
            EDNWriterFuncs.writeStream(ednObj, ms)
            ms.Position <- int64(0)
            let sr = new StreamReader(ms)
            let stringFromStream = sr.ReadToEnd()
            Funcs.Assert(teststr.Equals(stringFromStream))
            printf "Print String: %s" stringFromStream

    let testParseDirectory =
        let sw = System.Diagnostics.Stopwatch.StartNew()
        let rDir = EDNReaderFuncs.readDirectory("..\\..\\..\\TestData\\hierarchy-noisy")
        sw.Stop()
        printf "Elapsed ms: %i" sw.ElapsedMilliseconds