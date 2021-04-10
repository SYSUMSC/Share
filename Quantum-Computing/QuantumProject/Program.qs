namespace QuantumProject {
    open Microsoft.Quantum.Measurement;
    open Microsoft.Quantum.Random;
    open Microsoft.Quantum.Intrinsic;
    open Microsoft.Quantum.Arithmetic;
    open Microsoft.Quantum.Convert;
    open Microsoft.Quantum.Canon;
    open Microsoft.Quantum.Math;
    open Microsoft.Quantum.Oracles;
    open Microsoft.Quantum.Characterization;
    open Microsoft.Quantum.Diagnostics;

    @EntryPoint()
    operation Main() : Unit {
        for i in 1..99 {
            Message($"{RandomNumberInRange(15)}");
        }
    }

    operation RandomNumberInRange(max : Int) : Int {
        mutable bits = new Result[0];
        for idxBit in 1..BitSizeI(max) {
            mutable result = Zero;
            use q = Qubit()  {
                H(q);
                set result = MResetZ(q);
            }
            set bits += [result];
        }
        let sample = ResultArrayAsInt(bits);
        return sample;
    }
}
