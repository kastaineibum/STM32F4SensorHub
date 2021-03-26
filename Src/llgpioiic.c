#include "llgpioiic.h"
#include "dwt_stm32_delay.h"

extern I2C_HandleTypeDef hi2c2;
	  
void SDA_IN(void)
{
  GPIO_InitTypeDef GPIO_InitStruct = {0};

  /* GPIO Ports Clock Enable */
  __HAL_RCC_GPIOB_CLK_ENABLE();

  /*Configure GPIO pin : PB10 PB11 */
  GPIO_InitStruct.Pin = GPIO_PIN_11;
  GPIO_InitStruct.Mode = GPIO_MODE_INPUT;
  GPIO_InitStruct.Pull = GPIO_PULLUP;
  GPIO_InitStruct.Speed = GPIO_SPEED_FREQ_VERY_HIGH;
  HAL_GPIO_Init(GPIOB, &GPIO_InitStruct);
}

void SDA_OUT(void)
{
	GPIO_InitTypeDef GPIO_InitStruct = {0};

  /* GPIO Ports Clock Enable */
  __HAL_RCC_GPIOB_CLK_ENABLE();

  /*Configure GPIO pin : PB10 PB11 */
  GPIO_InitStruct.Pin = GPIO_PIN_11;
  GPIO_InitStruct.Mode = GPIO_MODE_OUTPUT_PP;
  GPIO_InitStruct.Pull = GPIO_PULLUP;
  GPIO_InitStruct.Speed = GPIO_SPEED_FREQ_VERY_HIGH;
  HAL_GPIO_Init(GPIOB, &GPIO_InitStruct);
}

void IIC_Init(void)
{					     
	GPIO_InitTypeDef GPIO_InitStruct = {0};

  /* GPIO Ports Clock Enable */
  __HAL_RCC_GPIOB_CLK_ENABLE();

  /*Configure GPIO pin : PB10 PB11 */
  GPIO_InitStruct.Pin = GPIO_PIN_10|GPIO_PIN_11;
  GPIO_InitStruct.Mode = GPIO_MODE_OUTPUT_PP;
  GPIO_InitStruct.Pull = GPIO_PULLUP;
  GPIO_InitStruct.Speed = GPIO_SPEED_FREQ_VERY_HIGH;
  HAL_GPIO_Init(GPIOB, &GPIO_InitStruct);
 
	IIC_SCL_1;
	IIC_SDA_1;

}

void IIC_Start(void)
{
	SDA_OUT();
	IIC_SDA_1;	  	  
	IIC_SCL_1;
	delay_us(4);
 	IIC_SDA_0;
	delay_us(4);
	IIC_SCL_0;
}

void IIC_Stop(void)
{
	SDA_OUT();
	IIC_SCL_0;
	IIC_SDA_0;
 	delay_us(4);
	IIC_SCL_1; 
	IIC_SDA_1;
	delay_us(4);							   	
}

uint8_t IIC_Wait_Ack(void)
{
	uint8_t ucErrTime=0;
	SDA_IN();
	IIC_SDA_1;
	delay_us(1);	   
	IIC_SCL_1;
	delay_us(1);	 
	while(READ_SDA)
	{
		ucErrTime++;
		delay_us(1);
		if(ucErrTime>250)
		{
			IIC_Stop();
			return 1;
		}
	}
	IIC_SCL_0;
	return 0;  
} 

void IIC_Ack(void)
{
	IIC_SCL_0;
	SDA_OUT();
	IIC_SDA_0;
	delay_us(2);
	IIC_SCL_1;
	delay_us(2);
	IIC_SCL_0;
}
	    
void IIC_NAck(void)
{
	IIC_SCL_0;
	SDA_OUT();
	IIC_SDA_1;
	delay_us(2);
	IIC_SCL_1;
	delay_us(2);
	IIC_SCL_0;
}					 				     
 
void IIC_Send_Byte(uint8_t txd)
{                        
    uint8_t t; 
    uint8_t t2;  
		SDA_OUT(); 	    
    IIC_SCL_0;
    for(t=0;t<8;t++)
    {              
      t2=(txd&0x80)>>7;
      if(t2==0)
      {
        IIC_SDA_0;
      }
      else
      {
        IIC_SDA_1;
      }
			txd<<=1; 	  
			delay_us(2);
			IIC_SCL_1;
			delay_us(2); 
			IIC_SCL_0;	
			delay_us(2);
    }	 
} 	    

uint8_t IIC_Read_Byte(unsigned char ack)
{
	unsigned char i,receive=0;
	SDA_IN();
  for(i=0;i<8;i++ )
	{
    IIC_SCL_0; 
    delay_us(2);
		IIC_SCL_1;
    receive<<=1;
    if(READ_SDA)receive++;   
		delay_us(1); 
  }
	/*  
	if(ack)
	{
		IIC_Ack();
	}
	*/
  return receive;
}

uint8_t i2cReadBuffer(uint8_t addr, uint8_t reg, uint8_t len, uint8_t* buf)
{
	IIC_Start();
	IIC_Send_Byte(addr << 1|I2C_Transmitter);
	if(IIC_Wait_Ack())
	{
		IIC_Stop();
		return false;
	}
	IIC_Send_Byte(reg);
	IIC_Wait_Ack();
	IIC_Start();
	IIC_Send_Byte(addr << 1 | I2C_Receiver);
    IIC_Wait_Ack();
	while (len) 
    {
        *buf = IIC_Read_Byte(1);
        if (len == 1)
            IIC_NAck();
        else
            IIC_Ack();
        buf++;
        len--;
    }
    IIC_Stop();
    return true;
}

uint8_t i2cReadBuffer2(uint8_t addr, uint8_t len, uint8_t* buf)
{
	IIC_Start();
	IIC_Send_Byte(addr << 1 | I2C_Receiver);
    IIC_Wait_Ack();
	while (len) 
    {
        *buf = IIC_Read_Byte(1);
        if (len == 1)
            IIC_NAck();
        else
            IIC_Ack();
        buf++;
        len--;
    }
    IIC_Stop();
    return true;
}



uint8_t i2cWriteBuffer(uint8_t addr, uint8_t reg, uint8_t len, uint8_t *data)
{
	int i;
	IIC_Start();
	IIC_Send_Byte(addr << 1|I2C_Transmitter);
	if(IIC_Wait_Ack())
	{
		IIC_Stop();
		return false;
	}
	IIC_Send_Byte(reg);
	IIC_Wait_Ack();
	for(i = 0; i < len; i++)
	{
		IIC_Send_Byte(data[i]);
		if(IIC_Wait_Ack())
		{
			IIC_Stop();
			return false;
		}
	}
	IIC_Stop();
	return true;
}

uint8_t i2cWriteBuffer2(uint8_t addr, uint8_t len, uint8_t *data)
{
	IIC_Start();
	IIC_Send_Byte(addr << 1|I2C_Transmitter);
	if(IIC_Wait_Ack())
	{
		IIC_Stop();
		return false;
	}
	for(int il = 0; il < len; il++)
	{
		IIC_Send_Byte(data[il]);
		if(IIC_Wait_Ack())
		{
			IIC_Stop();
			return false;
		}
	}
	IIC_Stop();
	return true;
}


	HAL_StatusTypeDef rev;
uint8_t i2cwrite(uint8_t addr, uint8_t reg, uint8_t len, uint8_t * data)
{
	/*
	rev = HAL_I2C_Mem_Write(&hi2c2,addr<<1,reg,len,data,len,1000);
	return rev;
	*/
	if(i2cWriteBuffer(addr,reg,len,data))
	{
		return TRUE;
	}
	else
	{
		return FALSE;
	}

}

uint8_t i2cwrite2(uint8_t addr, uint8_t len, uint8_t * data)
{
	/*
	rev = HAL_I2C_Mem_Write(&hi2c2,addr<<1,reg,len,data,len,1000);
	return rev;
	*/
	if(i2cWriteBuffer2(addr,len,data))
	{
		return TRUE;
	}
	else
	{
		return FALSE;
	}

}


uint8_t i2cread(uint8_t addr, uint8_t reg, uint8_t len, uint8_t *buf)
{
	/*	
	return HAL_I2C_Mem_Read(&hi2c2,addr<<1,reg,len,buf,len,1000);
	*/
	if(i2cReadBuffer(addr,reg,len,buf))
	{
		return TRUE;
	}
	else
	{
		return FALSE;
	}

}

uint8_t i2cread2(uint8_t addr, uint8_t len, uint8_t *buf)
{
	/*	
	return HAL_I2C_Mem_Read(&hi2c2,addr<<1,reg,len,buf,len,1000);
	*/
	if(i2cReadBuffer2(addr,len,buf))
	{
		return TRUE;
	}
	else
	{
		return FALSE;
	}

}

uint8_t iicDevReadByte(uint8_t devaddr,uint8_t addr)
{				  
	uint8_t temp=0;		  	    																 
	IIC_Start();  
	IIC_Send_Byte(devaddr);
	IIC_Wait_Ack(); 
	IIC_Send_Byte(addr);
	IIC_Wait_Ack();	

	IIC_Start();  	 	   
	IIC_Send_Byte(devaddr|1);
	IIC_Wait_Ack();	 
	temp=IIC_Read_Byte(0);			   
	IIC_Stop();
	return temp;
}

void iicDevRead(uint8_t devaddr,uint8_t addr,uint8_t len,uint8_t *rbuf)
{
	int i=0;
	IIC_Start();  
	IIC_Send_Byte(devaddr);  
	IIC_Wait_Ack();	
	IIC_Send_Byte(addr);
	IIC_Wait_Ack();	

	IIC_Start();  	
	IIC_Send_Byte(devaddr|1);  	
	IIC_Wait_Ack();	
	for(i=0; i<len; i++)
	{
		if(i==len-1)
		{
			rbuf[i]=IIC_Read_Byte(0);
		}
		else
			rbuf[i]=IIC_Read_Byte(1);
	}
	IIC_Stop( );	
}

void iicDevRead2(uint8_t addr, uint8_t reg, uint8_t len, uint8_t* buf)
{
		IIC_Start();
	IIC_Send_Byte(addr|I2C_Transmitter);
	if(IIC_Wait_Ack())
	{
		IIC_Stop();
		return;
	}
	IIC_Send_Byte(reg);
	IIC_Wait_Ack();
	IIC_Start();
	IIC_Send_Byte(addr|I2C_Receiver);
    IIC_Wait_Ack();
	while (len) 
    {
        *buf = IIC_Read_Byte(1);
        if (len == 1)
            IIC_NAck();
        else
            IIC_Ack();
        buf++;
        len--;
    }
    IIC_Stop();
    return;
}

void iicDevWriteByte(uint8_t devaddr,uint8_t addr,uint8_t data)
{				   	  	    																 
	IIC_Start();  
	IIC_Send_Byte(devaddr);
	IIC_Wait_Ack();	   
	IIC_Send_Byte(addr);
	IIC_Wait_Ack(); 	 										  		   
	IIC_Send_Byte(data);
	IIC_Wait_Ack();  		    	   
	IIC_Stop(); 
}

void iicDevWrite(uint8_t devaddr,uint8_t addr,uint8_t len,uint8_t *wbuf)
{
	int i=0;
	IIC_Start();  
	IIC_Send_Byte(devaddr);  	
	IIC_Wait_Ack();	
	IIC_Send_Byte(addr);
	IIC_Wait_Ack();	
	for(i=0; i<len; i++)
	{
		IIC_Send_Byte(wbuf[i]);  
		IIC_Wait_Ack();		
	}
	IIC_Stop( );	
}
