# How to use TrollTrack

TrollTrack is a fishing trip and catch log. Your trips, catches, and gear are stored **on your device** (not in the cloud). Use the bottom tabs to move between the main areas of the app.

---

## Dashboard

The **Dashboard** tab is your at-a-glance home screen.

**Location**  
You’ll see your current latitude and longitude (including a degrees–minutes–seconds style readout). The app needs **location permission** to show accurate coordinates and to support logging catches later.

**Weather**  
When location and weather data are available, the Dashboard shows current conditions such as temperature, wind, and sunrise/sunset times.

**Past trips**  
A list of recent trips appears below. **Tap a trip** to open its details, including a map of catch locations and the list of catches for that trip.

**Refresh**  
**Pull down** on the Dashboard to refresh location and related information.

---

## Trips

The **Trips** tab is where you **start and end trips**, **set up rods**, and **log catches**.

### When you do not have an active trip

You’ll see the **Start New Trip** form:

1. Enter a **trip name** (required).
2. Select the **target species**. If set, this will be the selected catch when a catch is recorded. If left as "Any", each recorded catch will prompt for the species caught.
3. Set the **trip date**, **water temperature**, **Secchi depth**, and **water clarity** using the pickers.
4. Optionally add **trip notes**.
5. Tap **Start Trip**.

**Before the trip is created**, TrollTrack shows a **location disclosure** screen. Read it, then tap **Continue** to proceed or **Cancel** to go back. You will then be asked for **location permission** if it hasn’t been granted yet. Location is used for catch coordinates, speed, and heading when you log catches.

After the trip starts, the form clears and the screen switches to **active trip** mode.

### When you have an active trip

**Trip banner**  
The top of the screen shows the active trip name and a short summary of catches by species.

**End Trip**  
Tap **End Trip** when you’re done fishing. Confirm if prompted. The trip is saved and is no longer “active.”

**Add Rod**  
Tap **+ Add Rod** to open the rod setup screen. There you can:

- Give the rod a **name**. The **name** will default to "Rod x" where x is the next number after the number of rods already defined.
- Enter **line out** (distance from boat to lure, in feet).
- Choose a **diver** from the list (if you use divers / inline weights).
- **Select a lure** from your lure library (you can open the lure picker and filters from here).

Save the rod when finished. You can add multiple rods for the same trip.

**Log a catch**  
Under **Log New Catch**, each configured rod appears as a tile showing the rod name and line out.

- **Tap** a rod briefly to **add a catch** for that rod. A **fish species** picker opens; choose a species to record the catch. The app uses a **fresh GPS fix** for that catch and saves coordinates (and related data) with the catch.
- **Press and hold** a rod to **edit** that rod’s setup (line out, diver, lure, etc.).

You must **select a fish species** in the picker to complete a catch. If no rods are set up, add a rod first.

**Catch history**  
Below the rods, **Catch History** lists catches for the active trip. **Tap a catch** to open **catch details** (species, time, position, lure, diver, line out, speed, heading, and related fields when available).

### Past trips (Trips tab)

When **no** trip is active, the **Trips** tab shows the start-trip form and, below it, your **past trips** list. **Tap a trip** to open its detail view (map and catches), same as from the Dashboard.

While a trip **is** active, that list is hidden on **Trips**—use the **Dashboard** to open a past trip, or **end the active trip** to return to the start-trip screen and the past-trips list.

---

## Lures

The **Lures** tab is your **personal lure library**.

**Browse**  
Lures appear as cards with photo (when set), manufacturer, description, buoyancy, length, and weight.

**View a photo**  
**Tap the lure image** to open a larger view; close it when you’re done.

**Add a lure**  
Tap **+ Add Lure** to open the add-lure flow. Fill in manufacturer, description, lure type, buoyancy, length, weight, colors, and images as needed, then save. New lures are available when you set up rods on a trip.

---

## About

The **About** tab shows the **TrollTrack** name, **version**, and a short description.

You can also use **Export Database** from this screen to save a copy of your data, similar to the export option under Settings.

---

## Tips

- Grant **location** when asked so catches and the Dashboard stay accurate.
- **Back up** your database occasionally using **Export Database** if you replace your phone or reinstall the app.
- If the **map** on a trip detail screen is blank, the app build must include a valid **Google Maps API key**; that is a setup issue for developers, not something you fix inside the app’s menus.
- Practice using the app by setting up a trip and then walk around your neighborhood and track catches, change lures and line out, etc. before hitting the water.
- Rod setups are saved between trips. Update lures, line out, etc. as needed instead of creating new rods each trip.

---

*This guide reflects the app’s tab structure. It intentionally does not cover tabs or features not listed above.*
