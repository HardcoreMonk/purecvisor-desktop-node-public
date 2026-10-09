// ADR-0016 integration tier: each test class creates real pcv-it- VMs through VMMS. Running the classes one after another
// keeps VM creation off the same second and off the same VMMS queue (2026-10-10 gate runs created two VMs in parallel).
[assembly: CollectionBehavior(DisableTestParallelization = true)]
