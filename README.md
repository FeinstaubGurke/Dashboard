# Dashboard for ELV LoRaWAN® Feinstaubsensor, ELV-LW-SPM

The dashboard processes the information received from the ELV LoRaWAN fine dust sensor via **The Things Network** and presents it collectively on a webpage.

[ELV LoRaWAN® Feinstaubsensor, ELV-LW-SPM](https://de.elv.com/p/elv-lorawan-feinstaubsensor-elv-lw-spm-P160408)

## The Things Network Config

### Integration

Create a new webhook with the *Webhook format* `JSON` set the base Url to `https://yourdomain.tld/webhook` end enable *Uplink message* to `/UplinkMessage` and *Join accept* to `/JoinAccept`.

### Api Key

Create a new api key for the Dashboard with *individual rights* and select `View devices in application`.
