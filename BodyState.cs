using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class BodyState
{
    private float weight;
    private float height;
    private bool isMale;
    private int age;
    private double fold1;
    private double fold2;
    private double fold3;

    public BodyState(float weight, float height) {
        this.height = height;
        this.weight = weight;
    }
    public BodyState(bool isMale, int age, double fold1, double fold2, double fold3)
    {
        this.isMale = isMale;
        this.age = age;
        this.fold1 = fold1;
        this.fold2 = fold2;
        this.fold3 = fold3;
    }

    public double CalculateBodyFatPercentage()
    {
        if (age.Equals(null))
        {
            Console.WriteLine("Невірне введення або виклик методу");
            return 0;
        }
        ;
        double sum = fold1 + fold2 + fold3;
        double bodyDensity;

        if (isMale)
        {
            // Формула Дуріна і Вомерслі для чоловіків
            bodyDensity = 1.10938 - (0.0008267 * sum) + (0.0000016 * sum * sum) - (0.0002574 * age);
        }
        else
        {
            // Формула Дуріна і Вомерслі для жінок
            bodyDensity = 1.0994921 - (0.0009929 * sum) + (0.0000023 * sum * sum) - (0.0001392 * age);
        }

        // Формула Сірі для визначення відсотку жиру
        double bodyFatPercentage = (495 / bodyDensity) - 450;

        return Math.Round(bodyFatPercentage, 1);
    }

    public double CalculateIMB()
    {
        if (height.Equals(null)) {
            Console.WriteLine("Невірне введення або виклик методу");
            return 0;
        };
        if (height > 10) height /= 100;
        Console.WriteLine(height);
        Console.WriteLine(weight);

        return weight / (height * height);
    }
}

