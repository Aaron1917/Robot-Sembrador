/**
* @author: Maltrax1917
* @param: number of test
*/

#ifndef sharp_h
#define sharp_h

void init_ADC(void);
uint16_t read_analog(void);
void measure_distance(uint8_t n, float *value);

#endif