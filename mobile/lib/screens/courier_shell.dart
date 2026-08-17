import 'package:flutter/material.dart';

import '../theme.dart';
import 'courier_home_screen.dart';
import 'my_deliveries_screen.dart';

class CourierShell extends StatefulWidget {
  const CourierShell({super.key});

  @override
  State<CourierShell> createState() => _CourierShellState();
}

class _CourierShellState extends State<CourierShell> {
  int _index = 0;

  void _goToOrders() => setState(() => _index = 1);

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: IndexedStack(
        index: _index,
        children: [
          CourierHomeScreen(onViewAllOrders: _goToOrders),
          const MyDeliveriesScreen(),
        ],
      ),
      bottomNavigationBar: NavigationBar(
        selectedIndex: _index,
        onDestinationSelected: (i) => setState(() => _index = i),
        backgroundColor: surfaceColor,
        indicatorColor: infoBg,
        destinations: const [
          NavigationDestination(icon: Icon(Icons.home_outlined), selectedIcon: Icon(Icons.home, color: primaryColor), label: 'Ana Sayfa'),
          NavigationDestination(icon: Icon(Icons.local_shipping_outlined), selectedIcon: Icon(Icons.local_shipping, color: primaryColor), label: 'Siparişler'),
        ],
      ),
    );
  }
}
