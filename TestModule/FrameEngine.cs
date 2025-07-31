using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestModule
{
    internal class FrameEngine
    {
        /*
         * 
         * 1. Вызов TryPlacePart()
         * Попытка разместить без поворота
         * Попытка разместить с 90 градусами
         * (Вызов TryPlace)
         * 
         * 2. TryPlace()
         * Поиск свободного пространства(цикл)
         * Смотрим по эвристике(Best Short Side Fit)
         * Находим лучшее совпадение
         * Размещаем в Frame
         * Делаем Split()
         *
         * 3. Split()
         * Делим область на вертикаль
         * Делим область на горизонталь
         * Урезаем лишнее(поглощенные)
         * MergeFreeSpace()
         * 
         * 4. MergeFreeSpace()
         * Удаляем поглощение
         * 
         */

        /*
         * 
         * 
         * 
         * 
         */
    }
}
