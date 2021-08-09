
String cmd = "";
void setup() {
  // put your setup code here, to run once:
  Serial.begin(115200);
 while (true){
    if(Serial.available() <= 0){
      cmd = Serial.readStringUntil('\n');
    }
    if(cmd == "START") break;
  }
}

void loop() {
  // put your main code here, to run repeatedly:
  float _angulo = (analogRead(A0) - 512.f)*360.f/1024.f;
   if(Serial.available()>0){
    cmd = Serial.readStringUntil('\n');
    if(cmd == "D") {
      Serial.print(_angulo);
      Serial.print('\n');
    }
  }
}
