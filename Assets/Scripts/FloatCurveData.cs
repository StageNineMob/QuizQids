namespace StageNine
{
    public class FloatCurveData
    {
        private float constantCoefficient;
        private float linearCoefficient;
        private float squareCoefficient;
        private float cubeCoefficient;

        public FloatCurveData()
        {
            constantCoefficient = linearCoefficient = squareCoefficient = cubeCoefficient = 0f;
        }

        public FloatCurveData(float startValue, float endValue, float startSlope, float endSlope)
        {
            constantCoefficient = startValue;
            linearCoefficient = startSlope;
            squareCoefficient = (3 * endValue) - endSlope - (2 * startSlope) - (3 * startValue);
            cubeCoefficient = endSlope - (2 * endValue) + startSlope + (2 * startValue);
        }

        public float GetFloatValue(float alpha)
        {
            return cubeCoefficient * alpha * alpha * alpha + squareCoefficient * alpha * alpha + linearCoefficient * alpha + constantCoefficient;
        }
    }
}
