# ParcelFlow — Guía de estudio del Dominio

> Objetivo: que entiendas **cada clase** del dominio, **por qué** está hecha así y **cómo** se relacionan.
> Léelo con el código abierto al lado. Cada sección indica el archivo.
>
> Estado: capa **Domain** de los 3 módulos terminada, **146 tests en verde**.
> Pendiente: Application, Infrastructure, API (ver §10).

---

## Índice
1. [Mapa del código y orden de lectura](#1-mapa-del-código-y-orden-de-lectura)
2. [Los 6 conceptos que tienes que dominar](#2-los-6-conceptos-que-tienes-que-dominar)
3. [BuildingBlocks: las piezas base](#3-buildingblocks-las-piezas-base)
4. [Módulo Shipments](#4-módulo-shipments)
5. [Módulo Tracking](#5-módulo-tracking)
6. [Módulo Deliveries](#6-módulo-deliveries)
7. [Cómo se conectan los módulos](#7-cómo-se-conectan-los-módulos)
8. [Técnicas de C# usadas (chuleta)](#8-técnicas-de-c-usadas-chuleta)
9. [Los tests](#9-los-tests)
10. [Decisiones tomadas y lo que falta](#10-decisiones-tomadas-y-lo-que-falta)
11. [Ejercicios para comprobar que lo entendiste](#11-ejercicios-para-comprobar-que-lo-entendiste)

---

## 1. Mapa del código y orden de lectura

```
Back/
├── src/
│   ├── ParcelFlow.BuildingBlocks.Domain/     ← piezas base compartidas por todos
│   │   ├── Entity.cs
│   │   ├── AggregateRoot.cs
│   │   ├── IDomainEvent.cs
│   │   ├── DomainException.cs
│   │   └── Guard.cs
│   │
│   ├── ParcelFlow.Modules.Shipments.Domain/   ← registrar y cancelar envíos
│   │   ├── Weight.cs, Dimensions.cs           (VOs que hiciste tú)
│   │   ├── Money.cs, TrackingNumber.cs, Address.cs, Contact.cs   (VOs)
│   │   ├── ShippingCostCalculator.cs          (Domain Service)
│   │   ├── ShipmentStatus.cs
│   │   ├── ShipmentDomainEvents.cs
│   │   └── Shipment.cs                        (AGGREGATE ROOT)
│   │
│   ├── ParcelFlow.Modules.Tracking.Domain/    ← historial de movimientos
│   │   ├── TrackingStatus.cs, TrackingStatusTransitions.cs  (máquina de estados)
│   │   ├── Location.cs, TrackingDescription.cs              (VOs)
│   │   ├── TrackingEvent.cs                                 (Entity)
│   │   ├── TrackingDomainEvents.cs
│   │   └── ShipmentTracking.cs                              (AGGREGATE ROOT)
│   │
│   └── ParcelFlow.Modules.Deliveries.Domain/  ← última milla
│       ├── DeliveryStatus.cs
│       ├── CourierId.cs, FailureReason.cs, DeliveryEvidence.cs  (VOs)
│       ├── DeliveryAttempt.cs                                   (Entity)
│       ├── DeliveryDomainEvents.cs
│       └── Delivery.cs                                          (AGGREGATE ROOT)
│
└── tests/  (un proyecto de tests por módulo)
```

**Dependencias entre proyectos** (quién conoce a quién):

```
Shipments.Domain ──┐
Tracking.Domain  ──┼──► BuildingBlocks.Domain
Deliveries.Domain ─┘

❌ Ningún módulo referencia a otro módulo.
```

### Orden de lectura recomendado
1. `BuildingBlocks` completo (son 5 archivos cortos).
2. `Money.cs` → es como tu `Weight` pero con 2 propiedades y operaciones.
3. `TrackingNumber.cs` → constructor privado + métodos de fábrica.
4. `Address.cs` y `Contact.cs` → uso de `Guard` y VO dentro de VO.
5. `ShippingCostCalculator.cs` → Domain Service.
6. **`Shipment.cs`** → el primer aggregate. Léelo despacio.
7. `TrackingStatusTransitions.cs` → máquina de estados.
8. **`ShipmentTracking.cs`** + `TrackingEvent.cs` → aggregate con colección de entidades.
9. `FailureReason.cs` → el otro estilo de VO (validación de varias propiedades juntas).
10. **`Delivery.cs`** + `DeliveryAttempt.cs` → el aggregate con más reglas.
11. Los tests de cada uno.

---

## 2. Los 6 conceptos que tienes que dominar

| Concepto | Pregunta que lo identifica | Igualdad | ¿Cambia? | Ejemplos en ParcelFlow |
|---|---|---|---|---|
| **Value Object** | ¿Me importa *cuál* es o solo *cuánto/qué* vale? → solo el valor | Por valores | No (se reemplaza) | `Weight`, `Money`, `Address`, `Location`, `FailureReason` |
| **Entity** | ¿Necesito distinguir dos con datos iguales? → sí | Por `Id` | Sí, por métodos | `TrackingEvent`, `DeliveryAttempt` |
| **Aggregate Root** | ¿Es la puerta de entrada que protege un grupo de reglas? | Por `Id` | Sí, solo por sus métodos | `Shipment`, `ShipmentTracking`, `Delivery` |
| **Domain Event** | ¿Algo que *ya pasó* y que a otros les interesa? | — | Nunca (es historia) | `ShipmentCreatedDomainEvent`, `ShipmentDeliveredDomainEvent` |
| **Domain Service** | ¿Regla que necesita varios objetos y no pertenece a ninguno? | — | Sin estado | `ShippingCostCalculator` |
| **Domain Exception** | ¿El dato es válido pero la operación no se permite *ahora*? | — | — | `DomainException("shipment.already_delivered")` |

### Las dos excepciones: ¿cuál uso?

```
¿El DATO es inválido por sí mismo?           → ArgumentException (o ArgumentOutOfRange/ArgumentNull)
   (peso -5, texto vacío, moneda "XX")          Ej: new Weight(-5)

¿El dato es válido pero el ESTADO no lo permite? → DomainException con Code
   (cancelar algo entregado, confirmar sin repartidor)  Ej: shipment.Cancel(...) cuando ya está Delivered
```

Más adelante, en la API, esto se traducirá a: `ArgumentException` → **400 Bad Request**, `DomainException` → **409 Conflict** (el `Code` se manda al front).

### Aggregate: la idea más importante
Un aggregate es un **grupo de objetos que se modifican juntos y cuyas reglas se protegen juntas**. Desde fuera:
- Solo se habla con la **raíz** (`Delivery`), nunca directamente con sus partes (`DeliveryAttempt`).
- Las partes se crean **dentro** de la raíz (por eso `DeliveryAttempt.Successful(...)` es `internal`).
- Entre aggregates distintos solo se guardan **Ids** (`Delivery.ShipmentId` es un `Guid`, no un objeto `Shipment`).

---

## 3. BuildingBlocks: las piezas base

Proyecto `ParcelFlow.BuildingBlocks.Domain`. Lo usan los 3 módulos. **No contiene reglas de negocio**: solo la "fontanería" del DDD.

### `Entity<TId>` — Entity.cs
```csharp
public abstract class Entity<TId> : IEquatable<Entity<TId>> where TId : notnull
{
    public TId Id { get; protected init; }
    // Equals compara: mismo tipo + mismo Id
}
```
- `abstract`: no se puede instanciar `Entity` "a secas", solo heredar.
- `<TId>` genérico: cada entidad elige el tipo de su Id (aquí siempre `Guid`).
- `protected init`: solo la propia entidad (o una hija) puede poner el Id, y solo al crearla.
- **Equals por Id**: es lo contrario a un `record`. Mira el test `TwoShipments_WithSameData_AreNotEqual_BecauseIdentityDiffers`.
- `GetType() != other.GetType()`: un `Shipment` y una `Delivery` con el mismo Guid **no** son iguales.

### `AggregateRoot<TId>` — AggregateRoot.cs
```csharp
public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    protected void RaiseDomainEvent(IDomainEvent e) => _domainEvents.Add(e);
    public void ClearDomainEvents() => _domainEvents.Clear();
}
```
- Un aggregate **es** una entidad (hereda de `Entity`) **más** una lista de eventos pendientes.
- `RaiseDomainEvent` es `protected`: solo el propio aggregate decide cuándo ocurre un evento.
- El aggregate **no publica** los eventos, solo los **apunta**. Más adelante, la Infrastructure los recogerá *después de guardar en BD* y los despachará. Así nunca se avisa de algo que al final no se guardó.
- `IAggregateRoot` (no genérica) existe para que el futuro `DbContext` pueda pedir "dame todos los aggregates que cambiaron" sin conocer sus tipos.

### `IDomainEvent` — IDomainEvent.cs
Solo exige `OccurredOnUtc`. Los eventos concretos son `sealed record` (inmutables, igualdad por valor, se escriben en una línea).

### `DomainException` — DomainException.cs
```csharp
public class DomainException(string code, string message) : Exception(message)
```
- Usa **constructor primario** (C# 12): los parámetros van junto al nombre de la clase.
- `Code` es estable (`"delivery.not_assigned"`); el mensaje puede cambiar o traducirse. Los tests comprueban el **código**, no el texto.

### `Guard` — Guard.cs
Aplica la **regla de tres** que vimos: validar textos se repetía en 6+ value objects, así que se extrajo.
```csharp
Guard.RequiredText(value, nameof(Street), maxLength: 200)   // → texto sin espacios sobrantes, o excepción
Guard.OptionalText(details, nameof(details), 500)            // → null si viene vacío
```
Fíjate que **devuelve** el valor limpio: por eso cabe en una línea dentro de un `init`, igual que tu `EnsureValid` de `Dimensions`.

> ¿Por qué `Weight` y `Dimensions` NO usan `Guard`? Porque validan **rangos numéricos** con reglas distintas (exclusivo vs inclusivo). Solo hay 2 casos → no se cumple la regla de tres.

---

## 4. Módulo Shipments

### 4.1 `Money` — Money.cs
**Qué es:** un importe + una moneda. Nunca existe un número de dinero "suelto".

**Reglas:**
- `Amount` ≥ 0 y siempre **redondeado a 2 decimales** (`MidpointRounding.AwayFromZero`: 10.005 → 10.01, como se redondea en contabilidad; el redondeo por defecto de .NET es "bancario" y daría 10.00).
- `Currency`: 3 letras, se normaliza a mayúsculas (`" mxn "` → `"MXN"`).

**Lo nuevo respecto a `Weight`: operaciones.**
```csharp
public Money Add(Money other)       // misma moneda, si no → InvalidOperationException
public Money Multiply(decimal f)    // f ≥ 0
public static Money operator +(Money a, Money b) => a.Add(b);
public static Money operator *(Money m, decimal f) => m.Multiply(f);
```
- Cada operación **devuelve un Money nuevo**; el original no cambia (test `Operations_DoNotModifyOriginal`). Igual que `string`.
- **Sobrecarga de operadores**: permite escribir `baseFee + weightFee` en lugar de `baseFee.Add(weightFee)`. Es azúcar; la regla vive en `Add`.
- ¿Por qué `InvalidOperationException` y no `DomainException` al sumar MXN + USD? Porque no es una regla del negocio que el usuario pueda violar: es un **error de programación** (nadie debería escribir ese código). 
- Como el redondeo está en el `init`, también `money with { Amount = 1.999m }` queda redondeado.

### 4.2 `TrackingNumber` — TrackingNumber.cs
**Formato:** `PF` + 10 caracteres de un alfabeto sin ambiguos (`0/O`, `1/I/L` fuera). Ej: `PF7K3M9QXR2A`.

**Patrón nuevo: constructor privado + métodos de fábrica.**
```csharp
private TrackingNumber(string value) { Value = value; }

public static TrackingNumber Generate()            // crear uno nuevo
public static TrackingNumber From(string value)    // reconstruir uno existente (usuario o BD)
```
Dos formas de "nacer" con intenciones distintas, y **nombres que lo dicen**. `new TrackingNumber(...)` no se puede usar desde fuera.

- `RandomNumberGenerator.GetItems<char>(Alphabet, 10)`: generador **criptográfico** (impredecible). `Random` normal sería predecible → alguien podría adivinar guías de otros clientes.
- **Unicidad**: el dominio genera guías con probabilidad de colisión despreciable (~8×10¹⁴ combinaciones), pero la garantía *absoluta* la dará la BD con un índice único (Infrastructure). El dominio no puede saber qué guías existen: eso requiere consultar la BD.
- `From` normaliza (`"  pf23456789ab "` → `"PF23456789AB"`): lo que escribe un usuario en el buscador funciona igual.

### 4.3 `Address` — Address.cs
5 propiedades, todas con el patrón de `Weight` (campo privado + `init` que valida), usando `Guard`:
- `Street`, `City`, `State`: obligatorias, con longitud máxima.
- `PostalCode`: 3–10 caracteres, solo letras/dígitos/espacio/guion (sirve para MX "64000", US "10001", CA "K1A 0B1").
- `Country`: ISO de 2 letras (`MX`).

Cada regla es de **una sola propiedad** → validar en el `init` funciona, y `with` también valida.

### 4.4 `Contact` — Contact.cs
Nombre + teléfono + **un `Address` dentro**. Es un **VO que contiene otro VO**: la igualdad del record compara también el `Address` por valor ("en profundidad").

**Decisión importante (desviación del documento de diseño):** el diseño proponía `Sender` y `Recipient` como **entidades**. Aquí son un **VO** (`Contact`), usado dos veces:
```csharp
public Contact Sender { get; private set; }
public Contact Recipient { get; private set; }
```
¿Por qué? Dentro de un envío, el remitente no tiene identidad ni ciclo de vida: es una **foto** de los datos al registrar el envío. Si el cliente cambia de teléfono mañana, los envíos viejos deben conservar el teléfono antiguo. Si algún día hay un módulo de *Clientes*, ahí **sí** sería una entidad (con Id), y el envío guardaría un `CustomerId` + la foto.

**Teléfono normalizado**: `"(81) 1234-5678"` → `"8112345678"`; `"+52 81..."` conserva el `+`. Guardar normalizado permite comparar y buscar sin depender de cómo lo escribió cada uno.

### 4.5 `ShippingCostCalculator` — ShippingCostCalculator.cs
**Domain Service**: la regla del costo necesita `Weight` **y** `Dimensions`; no es natural meterla en ninguno de los dos.

```
kilos facturables = máx(peso real, peso volumétrico)
costo             = 50 MXN + kilos facturables × 15 MXN
```
- Ejemplo 1: 5 kg en caja de 30×20×10 → volumétrico 1.2 → se cobran **5 kg** → 50 + 75 = **125 MXN**.
- Ejemplo 2: 2 kg en caja de 50×40×30 → volumétrico 12 → se cobran **12 kg** → 50 + 180 = **230 MXN**.

Aquí se ve **por qué** decidiste que `VolumetricWeight` devolviera `decimal`: se compara con `Weight.Kilograms`, no se convierte en un `Weight`.

Es `static` y **puro**: sin BD, sin reloj, sin aleatoriedad → mismo input, mismo output → test trivial.

### 4.6 `ShipmentStatus` — ShipmentStatus.cs
`Registered`, `Delivered`, `Cancelled`. Mucho más simple que los estados de Tracking, **a propósito**: cada bounded context modela solo lo que necesita para *sus* reglas. A Shipments solo le importa "¿se puede cancelar?".

Los enums tienen valores explícitos (`= 1`, `= 2`…) para que, cuando se guarden en BD como número, reordenar el enum no cambie el significado de los datos existentes.

### 4.7 `Shipment` — Shipment.cs ⭐ (Aggregate Root)

**Anatomía:**
```csharp
public sealed class Shipment : AggregateRoot<Guid>
{
    public TrackingNumber TrackingNumber { get; private set; }   // ← setters PRIVADOS
    public Contact Sender { get; private set; }
    // ...
    private Shipment() { }                                       // ← nadie hace "new Shipment()"

    public static Shipment Create(...)                           // ← única forma de nacer
    public void Cancel(string reason, DateTime cancelledAt)      // ← única forma de cancelarse
    public void MarkAsDelivered(DateTime deliveredAt)            // ← única forma de entregarse
}
```
Es una **clase** (no record) porque es una **entidad**: igualdad por Id y su estado cambia.

**`Create`:**
- Recibe VOs **ya válidos** (`Weight`, `Dimensions`...). Por eso el aggregate no revalida pesos: si existe un `Weight`, es válido. *Esta es la gran ventaja de los value objects.*
- **Calcula el costo él mismo** (no lo recibe) → nadie puede registrar un envío con un precio inventado.
- `Guid.CreateVersion7()`: Guid que empieza con la fecha → los Ids salen ordenados en el tiempo → los índices de la BD se mantienen eficientes (un Guid v4 aleatorio fragmenta el índice).
- Lanza `ShipmentCreatedDomainEvent` con lo que otros módulos necesitarán (Id, guía, ciudad origen/destino).

**`Cancel`:**
```csharp
if (Status == ShipmentStatus.Delivered)  throw new DomainException("shipment.already_delivered", ...);
if (Status == ShipmentStatus.Cancelled)  throw new DomainException("shipment.already_cancelled", ...);
```
- Primero se comprueban las reglas, **después** se cambia el estado. Si algo falla, el objeto queda intacto.
- No se borra nada: queda `CancelledAt` y `CancellationReason` ("la cancelación debe conservar el historial").

**`MarkAsDelivered` es idempotente:**
```csharp
if (Status == ShipmentStatus.Delivered) return;   // segunda llamada: no hace nada
```
Lo llamará un evento del módulo Deliveries. Los eventos **pueden llegar repetidos** (reintentos), así que quien los consume debe tolerar recibirlos dos veces. Compáralo con `Cancel`, que **sí** lanza error la segunda vez: ahí es un usuario quien lo pide, y debe enterarse.

**Fechas como parámetro:** `Cancel(reason, cancelledAt)` en vez de usar `DateTime.UtcNow` dentro. Así el dominio es **determinista** y los tests usan una fecha fija (`TestData.Now`). Más adelante, la capa Application le pasará la hora real con `TimeProvider`.

---

## 5. Módulo Tracking

### 5.1 La máquina de estados — TrackingStatus.cs + TrackingStatusTransitions.cs

```
Created ─► PickedUp ─► InTransit ─► AtDistributionCenter ─► OutForDelivery ─► Delivered
                           ▲                 │                   │    ▲
                           └─────────────────┘                   ▼    │
                    (puede pasar por varios centros)       DeliveryFailed

Cualquier estado NO final ─► Cancelled          Finales: Delivered, Cancelled
```

La tabla es un `Dictionary<estado, estados permitidos después>`:
```csharp
[TrackingStatus.OutForDelivery] = [TrackingStatus.Delivered, TrackingStatus.DeliveryFailed, TrackingStatus.Cancelled],
```
- **Datos en vez de `if`s**: toda la regla se lee de un vistazo. Con `if/else` serían 20 líneas difíciles de revisar.
- Separada del aggregate → se prueba sola (`TrackingStatusTransitionsTests`) y el front podrá preguntar "¿qué estados siguen?" (`NextStatuses`) para mostrar solo los botones válidos.
- Regla del diseño "no se puede entregar sin haber salido a reparto": `Delivered` solo aparece en la lista de `OutForDelivery`.
- `IsFinal` = "no tiene siguientes". El test `EveryStatus_HasTransitionsDefined` avisa si alguien añade un estado al enum y olvida la tabla.

### 5.2 `Location` y `TrackingDescription`
VOs de una propiedad de texto, mismo patrón que `Weight` pero con `Guard`. Son tipos propios (y no `string`) para que el compilador impida pasar una descripción donde va una ubicación.

### 5.3 `TrackingEvent` — TrackingEvent.cs (Entity)
- Es **entidad**: dos movimientos con el mismo estado, lugar y hora siguen siendo **dos** movimientos (por ejemplo, dos escaneos seguidos).
- **Inmutable**: setters privados y ningún método que lo cambie ("los movimientos históricos son inmutables").
- Constructor **`internal`**: solo el código del mismo proyecto (el aggregate) puede crear movimientos. Desde la API no se podrá hacer `new TrackingEvent(...)`.

### 5.4 `ShipmentTracking` — ShipmentTracking.cs ⭐ (Aggregate Root)

**Patrón nuevo: colección privada expuesta como solo lectura.**
```csharp
private readonly List<TrackingEvent> _events = [];
public IReadOnlyList<TrackingEvent> Events => _events.AsReadOnly();
```
Desde fuera se puede **leer** el historial pero no `Add`, `Remove` ni `Clear`. La única forma de añadir es `RegisterEvent`, que valida. `AsReadOnly()` devuelve un envoltorio: ni siquiera casteando a `List` se accede a la lista interna (test `Events_CannotBeModifiedFromOutside`).

**No conoce Shipments:** guarda `ShipmentId` (Guid) y `TrackingNumber` como `string`, no el VO `TrackingNumber` de Shipments. Un módulo **no referencia** el dominio de otro; solo recibe datos simples a través de eventos.

**`RegisterEvent` — el orden importa:**
1. ¿La transición es válida? → si no, `DomainException("tracking.invalid_transition")`.
2. ¿La fecha es posterior al último movimiento? → si no, `"tracking.event_out_of_order"`.
3. Solo entonces: añade el movimiento, actualiza estado/ubicación/fecha, lanza eventos.

Lanza **siempre** `ShipmentStatusChangedDomainEvent`, y **además** `ShipmentArrivedAtCenterDomainEvent` cuando llega a un centro: según el diseño, ese es el momento en que Deliveries debe crear la entrega pendiente.

**Regla que NO está aquí:** "no se pueden registrar movimientos para guías inexistentes". El aggregate no puede saber qué guías existen (requiere BD). Esa comprobación irá en el handler de Application: "busca el tracking por guía; si no existe → 404".

---

## 6. Módulo Deliveries

### 6.1 Estados — DeliveryStatus.cs
```
Pending ─► Assigned ─► Delivered
   ▲          │
   └──────────┘  intento fallido (quedan intentos → vuelve a Pending, hay que reasignar)
              │
              └─► Failed     (3er intento fallido, final)

Pending / Assigned ─► Cancelled (el envío se canceló)
```
**Decisión:** tras un fallo, la entrega vuelve a **Pending** y pierde el repartidor. Así el siguiente intento *obliga* a asignar de nuevo (normalmente en la ruta del día siguiente), y la regla "no se confirma sin repartidor" se cumple siempre. Además, el evento de nueva asignación es el que (en el futuro) devolverá el tracking a `OutForDelivery`.

### 6.2 `CourierId`
VO de texto. No hay módulo de repartidores (fuera de alcance), así que es solo un identificador validado. Un tipo propio evita confundirlo con cualquier otro `string`.

### 6.3 `FailureReason` — FailureReason.cs ⭐ (el otro estilo de VO)
Regla: **"si el código es `Other`, los detalles son obligatorios"**. Esta regla mira **dos propiedades a la vez**: es exactamente el problema que vimos con `Dimensions` y el volumen.

Con el estilo de `Weight` (validar en `init`) no funciona: al ejecutarse el `init` de `Code`, `Details` aún no tiene valor. Solución:
```csharp
public DeliveryFailureCode Code { get; }      // ← solo get, SIN init
public string? Details { get; }

public FailureReason(DeliveryFailureCode code, string? details = null)
{
    // valida TODO junto aquí
}
```
Al no haber `init`, **`reason with { Code = ... }` no compila**. La única puerta es el constructor, y el constructor siempre valida.

| Estilo | Cuándo usarlo | Ejemplos |
|---|---|---|
| Campo privado + `init` que valida | Todas las reglas son de **una** propiedad | `Weight`, `Dimensions`, `Money`, `Address` |
| `{ get; }` + validación en constructor | Hay reglas que cruzan **varias** propiedades | `FailureReason`, `DeliveryEvidence` |

`Enum.IsDefined(code)`: en C# un enum acepta cualquier número (`(DeliveryFailureCode)999` compila). Esta línea lo impide.

### 6.4 `DeliveryEvidence`
Quién recibió (`ReceivedBy`, obligatorio) + notas opcionales. `Guard.OptionalText` convierte `"   "` en `null`: un solo valor para "no hay notas".

### 6.5 `DeliveryAttempt` — DeliveryAttempt.cs (Entity)
Un intento es **exitoso** (con `Evidence`) **o** **fallido** (con `FailureReason`), nunca las dos cosas. Para garantizarlo, en vez de un constructor con todos los parámetros opcionales hay **dos fábricas con nombre**:
```csharp
internal static DeliveryAttempt Successful(int n, CourierId c, DeliveryEvidence e, DateTime at)
internal static DeliveryAttempt Failed(int n, CourierId c, FailureReason r, DateTime at)
```
Es imposible construir un intento "a medias". Cada intento guarda **qué repartidor** lo hizo (útil cuando hay reasignaciones).

### 6.6 `Delivery` — Delivery.cs ⭐ (Aggregate Root)

**Dónde se cumple cada regla del diseño:**

| Regla del diseño | Dónde |
|---|---|
| Solo se asigna cuando está listo para reparto | La `Delivery` solo se **crea** cuando Tracking avisa (futuro handler) |
| Exitosa registra fecha y receptor | `ConfirmDelivery(DeliveryEvidence, DateTime)` |
| Fallida registra motivo | `ReportFailure(FailureReason, DateTime)` |
| Entregado no vuelve a reparto | `AssignCourier` → `EnsureNotFinished()` |
| Máximo 3 intentos | `MaxAttempts` + estado `Failed` |
| No se confirma sin repartidor | `EnsureAssigned()` |

**Métodos privados de verificación** (`EnsureNotFinished`, `EnsureAssigned`): las mismas comprobaciones se necesitan en varios métodos públicos → DRY dentro del aggregate. `EnsureAssigned` llama primero a `EnsureNotFinished`, así el error es el más preciso posible (`max_attempts_reached` en vez de un genérico `not_assigned`).

**`RemainingAttempts`**: propiedad calculada. No se guarda: se deriva del número de intentos. Un dato derivado guardado aparte acaba desincronizándose.

**`Cancel` es idempotente** (si ya está cancelada o fallida, no hace nada) porque lo disparará el evento `ShipmentCancelled` de otro módulo.

---

## 7. Cómo se conectan los módulos

Los módulos **no se llaman entre sí**: se avisan mediante eventos. Este es el flujo completo que se implementará en la capa Application/Infrastructure (los eventos de dominio ya existen):

```
 SHIPMENTS                         TRACKING                            DELIVERIES
 ─────────                         ────────                            ──────────
 Shipment.Create()
   └─ ShipmentCreated ───────────► ShipmentTracking.Start()
                                   (estado Created)
                                   RegisterEvent(PickedUp)...
                                   RegisterEvent(AtDistributionCenter)
                                     └─ ShipmentArrivedAtCenter ──────► Delivery.CreatePending()
                                                                        AssignCourier()
                                   RegisterEvent(OutForDelivery) ◄───── DeliveryAssigned
                                                                        ReportFailure()
                                   RegisterEvent(DeliveryFailed) ◄───── DeliveryAttemptFailed
                                                                        ConfirmDelivery()
 Shipment.MarkAsDelivered() ◄───┬─ RegisterEvent(Delivered) ◄────────── ShipmentDelivered
                                │
 Shipment.Cancel()              │
   └─ ShipmentCancelled ────────┴─► RegisterEvent(Cancelled)
                       └─────────────────────────────────────────────► Delivery.Cancel()
```

**Domain Event vs Integration Event** (para la siguiente fase):
- **Domain Event** (lo que hay ahora): interno al módulo, usa tipos del dominio (ej. `TrackingStatus`).
- **Integration Event**: contrato **público** entre módulos, solo con tipos simples (`Guid`, `string`, `DateTime`). Irá en un proyecto `*.IntegrationEvents` por módulo. Un handler de Application traducirá uno en otro.

Separarlos permite cambiar el dominio interno sin romper a los otros módulos.

---

## 8. Técnicas de C# usadas (chuleta)

| Técnica | Ejemplo | Para qué |
|---|---|---|
| `sealed record` | `public sealed record Money` | VO: igualdad por valor gratis, sin herencia |
| Campo privado + `init` que valida | `init => _length = EnsureValid(value, ...)` | Validar también en `with { }` |
| `{ get; }` + constructor | `FailureReason` | Validar reglas entre varias propiedades; `with` no compila |
| `readonly` en campos | `private readonly decimal _amount;` | Solo se asigna al construir |
| Constructor privado + fábricas estáticas | `TrackingNumber.Generate()` / `From()` | Formas de crear con nombre e intención |
| `internal` | `internal TrackingEvent(...)` | Solo el aggregate (mismo proyecto) crea sus partes |
| `private set` | `public ShipmentStatus Status { get; private set; }` | Se lee desde fuera, se cambia solo por métodos |
| Constructor privado vacío | `private Shipment() { }` | Nadie crea un aggregate inválido (y EF lo usará después) |
| `List` privada + `IReadOnlyList` pública | `_events` / `Events` | Colección que solo el aggregate modifica |
| Propiedad calculada | `public decimal Volume => L * W * H;` | Datos derivados, sin duplicar estado |
| Sobrecarga de operadores | `operator +(Money, Money)` | Código de negocio más legible |
| Constructor primario (C# 12) | `class DomainException(string code, ...)` | Menos código repetido |
| Expresiones de colección (C# 12) | `List<X> _events = [];`, `[A, B]` | Sintaxis corta para listas/arrays |
| Patrones `is ... or ...` | `Status is DeliveryStatus.Cancelled or DeliveryStatus.Failed` | Comparar con varios valores legiblemente |
| `ArgumentNullException.ThrowIfNull(x)` | en `Shipment.Create` | Validar null en una línea |
| `Guid.CreateVersion7()` | Ids de aggregates/entities | Ids ordenados en el tiempo |
| `RandomNumberGenerator` | `TrackingNumber.Generate()` | Aleatoriedad impredecible (seguridad) |
| `Enum.IsDefined` | `FailureReason` | Evitar enums con valores inventados |
| `MidpointRounding.AwayFromZero` | `Money` | Redondeo "contable" (no bancario) |

---

## 9. Los tests

| Proyecto | Archivos | Qué cubren |
|---|---|---|
| Shipments.UnitTests (83) | `WeightTests`, `DimensionsTests`, `MoneyTests`, `TrackingNumberTests`, `AddressAndContactTests`, `ShippingCostCalculatorTests`, `ShipmentTests`, `TestData` | VOs, cálculo de costo, reglas de Shipment |
| Tracking.UnitTests (37) | `TrackingStatusTransitionsTests`, `ShipmentTrackingTests`, `TrackingValueObjectsTests` | Máquina de estados, historial, orden cronológico, eventos |
| Deliveries.UnitTests (26) | `DeliveryValueObjectsTests`, `DeliveryTests` | VOs, asignación, confirmación, 3 intentos, cancelación |

**Patrones usados en los tests:**
- **Object Mother** (`TestData.cs` en Shipments, helpers `StartTracking()`, `AssignedDelivery()` en los otros): crea objetos válidos para que cada test escriba **solo lo que le importa**.
- **`[Theory]` + `[InlineData]`** para casos con datos simples (strings, enums); **`TheoryData`** para `decimal` (`DimensionsTests`).
- **Se comprueba el `Code`** de las `DomainException`, no el mensaje: el mensaje puede cambiar, el código es contrato.
- **Tests que documentan decisiones**: `TwoShipments_WithSameData_AreNotEqual_BecauseIdentityDiffers`, `VolumetricWeight_CanExceedMaxPhysicalWeight`, `MarkAsDelivered_Twice_IsIdempotent`.
- **Comprobar que el estado NO cambió tras un error** (`Assert.Single(tracking.Events)` tras una transición inválida).
- **Fecha fija** (`Now = 2026-11-19 10:00 UTC`): los tests nunca dependen del reloj real.

Ejecutar: `dotnet test` desde `ParcelFlow\Back`.

---

## 10. Decisiones tomadas y lo que falta

### Decisiones que tomé (puedes cuestionarlas)

| Tema | Decisión | Alternativa descartada |
|---|---|---|
| Sender/Recipient | VO `Contact` (foto de datos) | Entidades (diseño original) |
| Moneda del costo | MXN, tarifa base 50 + 15/kg | Multi-moneda |
| Redondeo | 2 decimales, AwayFromZero, en el `init` | Rechazar importes con >2 decimales |
| Formato de guía | `PF` + 10 chars sin ambiguos | Numérico secuencial (predecible) |
| País / teléfono | ISO-2 / 7–15 dígitos normalizado | Validación por país |
| ShipmentStatus | Solo Registered/Delivered/Cancelled | Copiar los estados de Tracking |
| Tracking: varios centros | `AtDistributionCenter ↔ InTransit` permitido | Un solo centro |
| Tracking: eventos del diseño | `ShipmentStatusChanged` + `ShipmentArrivedAtCenter` | `PackageScanned`, `ShipmentDispatched` (redundantes con StatusChanged) |
| Tracking: commands | Solo `RegisterTrackingEvent` (siempre deja historial) | `UpdateShipmentStatus` aparte (cambiaría estado sin movimiento) |
| Fallo de entrega | Vuelve a `Pending`, hay que reasignar | Seguir `Assigned` con el mismo repartidor |
| DeliveryAssignment | Campos `CourierId`/`AssignedAt` en Delivery + courier en cada intento | Entidad separada (diseño original) |
| Fechas | Parámetro UTC en cada método | `DateTime.UtcNow` dentro del dominio |
| Ids | `Guid` v7 | Ids fuertemente tipados (`ShipmentId`) — buena mejora futura |

### Lo que falta para terminar el backend
1. **Application** (por módulo): commands/queries + handlers, **mediator propio**, DTOs, interfaces de repositorio, handlers de eventos.
2. **IntegrationEvents** (por módulo): contratos públicos.
3. **Infrastructure**: EF Core + PostgreSQL (un `DbContext` y schema por módulo), mapeo de VOs, repositorios, despacho de eventos después de `SaveChanges`.
4. **API**: Minimal APIs, `ProblemDetails`, mapeo `ArgumentException`→400, `DomainException`→409, OpenAPI, CORS para el front.
5. **Tests**: integración (Testcontainers + PostgreSQL) y arquitectura (NetArchTest).

---

## 11. Ejercicios para comprobar que lo entendiste

Hazlos sin mirar este documento. Si un test se rompe, ¡bien!: entiende por qué y deshaz el cambio.

1. **Romper a propósito:** en `TrackingStatusTransitions`, añade `TrackingStatus.Delivered` a la lista de `AtDistributionCenter`. ¿Qué test falla y qué regla de negocio representa?
2. **Explica con tus palabras:** ¿por qué `FailureReason` no usa `init` y `Money` sí?
3. **Predice:** ¿qué pasa con `new Money(10m, "MXN") with { Amount = 3.14159m }`? Compruébalo en un test.
4. **Diseña:** el negocio quiere un recargo de 30 MXN si el destino es otro país. ¿Dónde va esa regla? ¿Qué clases cambian? ¿Qué tests escribirías primero?
5. **Detecta:** ¿por qué `ShipmentTracking` guarda `TrackingNumber` como `string` y no usa el `TrackingNumber` de Shipments? ¿Qué pasaría si lo hiciera?
6. **Idempotencia:** ¿por qué `Shipment.MarkAsDelivered` no lanza error la segunda vez y `Shipment.Cancel` sí?
7. **Nuevo VO:** crea `ShipmentId` como `sealed record` que envuelva un `Guid` (no vacío). ¿Qué ganarías usándolo en vez de `Guid` en `Delivery.ShipmentId`?
8. **Reto:** añade la regla "un movimiento no puede tener fecha futura". ¿Dónde la pondrías y por qué el aggregate necesitaría recibir *la hora actual* además de `occurredAt`?
