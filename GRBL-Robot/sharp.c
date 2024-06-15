/*
 sharp.c - sharp control metods
  
*/
#include "grbl.h"

void init_ADC(void)
{
    // Ajustar referencia de voltaje a AVCC (5V)
    //ADMUX |= (1 << REFS0); // REFS0 = 1 (Vref as AVCC pin)
    ADMUX = 0x20;
    // Habilitar ADC y prescaler de 128 (para obtener una frecuencia de muestreo de aproximadamente 125kHz)
    ADCSRA |= (1 << ADEN) | (1 << ADPS2) | (1 << ADPS1) | (1 << ADPS0);

    // Inicializar como entrada A4
    SENSOR_DDR &= ~(1 << SENSOR_BIT);
    // Habilitar resistencia pull-up interna
    // SENSOR_PORT |= (1 << SENSOR_BIT);
}

uint16_t read_analog(void)
{
    // Limpiar los primeros 4 bits del registro ADMUX (para asegurarse de que el canal esté seleccionado correctamente)
    // Seleccionar el canal de ADC
    ADMUX = 0x40 | (SENSOR_BIT & 0x07); // 0100 0000 | (0000 0100 & 0000 0111)

    // Iniciar la conversión ADC
    ADCSRA |= (1 << ADSC);

    // Esperar hasta que se complete la conversión
    while (!(ADCSRA & (1 << ADIF)));

    // Limpiar la bandera de interrupcion
    ADCSRA |= (1<<ADIF);
	_delay_ms(1);

    // Devolver el resultado de la conversión ADC
    return ADC;
}

// time to measure distance 2~3 ms with n = 20
void measure_distance(uint8_t n, float *value)
{
    uint32_t suma = 0;
    for (int i = 0; i < n; i++)
    {
        suma = suma + read_analog();
    }
    float adc = suma / n;
    *value = 17569.7 * pow(adc, -1.2062);
}