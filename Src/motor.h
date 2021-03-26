#ifndef __MOTOR_H
#define __MOTOR_H
#include "dwt_stm32_delay.h"
#include "stm32f4xx_hal.h"

#define MOTORTICKGAP 	2 //miliseconds
#define BACKSTEP 			3 //go back when touch stopswitch
#define LSTOPPIN 			6 //left touch stopswitch pin in motorpin
#define RSTOPPIN 			7 //right touch stopswitch pin in motorpin

extern uint16_t motorpin[16][8];

void motor_initpin(uint16_t motorid,uint16_t bgrb,uint16_t pinnum);
void motor42_fround(uint16_t motorid,uint32_t round);
void motor42_rround(uint16_t motorid,uint32_t round);
void motor42_forward(uint16_t motorid,uint32_t step);
void motor42_reverse(uint16_t motorid,uint32_t step);

GPIO_TypeDef* getiogroup(uint16_t pinnum);
uint16_t getiopin(uint16_t pinnum);
void setioout(uint16_t pinnum);
void setioin(uint16_t pinnum);
void setiohigh(uint16_t pinnum);
void setiolow(uint16_t pinnum);
uint8_t getiostate(uint16_t pinnum);

#endif

